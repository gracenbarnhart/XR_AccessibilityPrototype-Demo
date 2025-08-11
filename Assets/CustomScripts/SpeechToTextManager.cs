using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SpeechToTextManager : MonoBehaviour
{
    public static SpeechToTextManager Instance { get; private set; }

    // -------------------- Config --------------------
    [Header("Config Source")]
    [Tooltip("Load Azure key & region from Assets/StreamingAssets/azure_speech.json")]
    public bool loadFromJson = true;
    public string jsonFileName = "azure_speech.json";

    [Header("Azure Speech Settings (fallback if JSON missing)")]
    public string azureKey;
    public string azureRegion = "westeurope";

    [Header("Capture")]
    [Tooltip("Preferred mic sample rate. 0 = device default")]
    public int preferredSampleRate = 16000;
    [Tooltip("Seconds per chunk sent to Azure")]
    public float clipLength = 4f;
    [Tooltip("Rolling mic buffer length (seconds)")]
    public int rollingBufferSeconds = 10;

    [Header("Quality / Parsing")]
    public bool useDetailedFormat = true;

    [Header("Silence Gate (debug)")]
    public bool gateSilence = false;             // OFF while debugging
    public float silenceRmsThreshold = 0.001f;   // start low
    public bool logRms = true;

    [Header("Mic Selection")]
    [Tooltip("Set to >= 0 to force a device index. -1 = auto-pick by probing RMS")]
    public int micIndex = -1;
    [Tooltip("Allow cycling devices at runtime with [ and ]")]
    public bool allowRuntimeMicCycle = true;

    // -------------------- API --------------------
    public delegate void OnTranscription(string text, int speakerId);
    public event OnTranscription OnCaption;

    public void SimulateCaption(string txt, int id)
    {
        Debug.Log($"[STT] SimulateCaption: “{txt}” from speaker {id}");
        OnCaption?.Invoke(txt, id);
    }

    // -------------------- Private --------------------
    private string[] devices = Array.Empty<string>();
    private int currentMicIndex = -1;
    private string currentMicName = null;
    private AudioClip micClip;
    private AudioSource monitorSource; // muted monitor keeps data flowing

    [Serializable] private class AzureJson { public string speechKey; public string region; }
    [Serializable] private class AzureSimple { public string RecognitionStatus; public string DisplayText; }
    [Serializable] private class AzureDetailedNBest { public string Display; }
    [Serializable] private class AzureDetailed { public string RecognitionStatus; public AzureDetailedNBest[] NBest; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadFromJson) TryLoadJson();

        monitorSource = GetComponent<AudioSource>();
        if (monitorSource == null) monitorSource = gameObject.AddComponent<AudioSource>();
        monitorSource.loop = true;
        monitorSource.playOnAwake = false;
        monitorSource.mute = true;
    }

    void Start()
    {
        devices = Microphone.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("[STT] No microphones found. Check Windows Privacy > Microphone and your input device.");
            enabled = false; return;
        }

        Debug.Log("[STT] Found microphones:");
        for (int i = 0; i < devices.Length; i++) Debug.Log($"  [{i}] {devices[i]}");

        if (micIndex >= 0 && micIndex < devices.Length)
        {
            StartCoroutine(BeginMic(devices[micIndex]));
        }
        else
        {
            // Auto-pick: probe all devices and choose the one with highest RMS
            StartCoroutine(AutoPickMicByRms());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            SimulateCaption("Demo caption (hotkey N).", 0);

        if (!allowRuntimeMicCycle || devices.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.RightBracket)) // ]
            CycleMic(+1);
        else if (Input.GetKeyDown(KeyCode.LeftBracket)) // [
            CycleMic(-1);
    }

    void CycleMic(int dir)
    {
        int next = (currentMicIndex + dir + devices.Length) % devices.Length;
        Debug.Log($"[STT] Switching mic to [{next}] {devices[next]}");
        StopCurrentMic();
        StartCoroutine(BeginMic(devices[next]));
    }

    IEnumerator AutoPickMicByRms()
    {
        int bestIndex = -1;
        float bestRms = -1f;

        for (int i = 0; i < devices.Length; i++)
        {
            string name = devices[i];

            // Skip obvious virtual/Oculus devices if we can
            string lower = name.ToLowerInvariant();
            if (lower.Contains("oculus") || lower.Contains("virtual") || lower.Contains("vb-audio"))
            {
                Debug.Log($"[STT] Skipping virtual device: {name}");
                continue;
            }

            // Probe ~0.6s
            int rate = preferredSampleRate <= 0 ? 0 : preferredSampleRate;
            var testClip = Microphone.Start(name, true, 1, rate);
            float t0 = Time.time;
            while (Microphone.GetPosition(name) <= 0 && Time.time - t0 < 1f) yield return null;
            yield return new WaitForSeconds(0.2f);

            int frames = Mathf.Min(2048, testClip.samples);
            float[] buf = new float[Mathf.Max(1, frames)];
            testClip.GetData(buf, 0);
            float rms = ComputeRms(buf);
            Debug.Log($"[STT] Probe [{i}] {name} -> RMS={rms:F4}, freq={testClip.frequency}, ch={testClip.channels}");

            if (rms > bestRms) { bestRms = rms; bestIndex = i; }

            Microphone.End(name);
        }

        if (bestIndex < 0)
        {
            // Fallback: pick first
            bestIndex = 0;
            Debug.LogWarning("[STT] No suitable mic found via RMS probe; using first device.");
        }

        Debug.Log($"[STT] Auto-picked mic: [{bestIndex}] {devices[bestIndex]} (RMS={bestRms:F4})");
        yield return BeginMic(devices[bestIndex]);
    }

    IEnumerator BeginMic(string name)
    {
        currentMicName = name;
        currentMicIndex = Array.IndexOf(devices, name);

        int rate = preferredSampleRate <= 0 ? 0 : preferredSampleRate;
        micClip = Microphone.Start(currentMicName, true, Mathf.Max(rollingBufferSeconds, 2), rate);

        // Wait until mic writes something
        float t0 = Time.time;
        while (Microphone.GetPosition(currentMicName) <= 0 && Time.time - t0 < 2f) yield return null;

        if (micClip == null)
        {
            Debug.LogError($"[STT] Failed to start mic: {currentMicName}");
            yield break;
        }

        Debug.Log($"[STT] Mic started: {currentMicName}, clipFreq={micClip.frequency}, ch={micClip.channels}, len={micClip.samples}");

        monitorSource.clip = micClip;
        monitorSource.Play();

        // Start transcription loop (restart if already running)
        StopCoroutineSafe(ContinuousTranscribe());
        StartCoroutine(ContinuousTranscribe());
    }

    void StopCurrentMic()
    {
        if (!string.IsNullOrEmpty(currentMicName)) Microphone.End(currentMicName);
        if (monitorSource != null) monitorSource.Stop();
        micClip = null;
    }

    IEnumerator ContinuousTranscribe()
    {
        yield return new WaitForSeconds(1f);
        var wait = new WaitForSeconds(clipLength);

        while (true)
        {
            yield return wait;

            var chunk = MakeRecentChunk(micClip, clipLength, currentMicName);
            if (chunk == null) continue;

            // RMS probe
            float[] probe = new float[chunk.samples];
            chunk.GetData(probe, 0);
            float rms = ComputeRms(probe);
            if (logRms) Debug.Log($"[STT] RMS={rms:F4}");
            if (gateSilence && rms < silenceRmsThreshold)
            {
                if (logRms) Debug.Log("[STT] Skipping chunk (below threshold).");
                continue;
            }

            string _;
            byte[] wav = WavUtility.FromAudioClip(chunk, out _, saveAsFile: false);

            yield return StartCoroutine(TranscribeWithAzure(wav, chunk.frequency, text =>
            {
                if (!string.IsNullOrEmpty(text))
                    OnCaption?.Invoke(text, 0);
            }));
        }
    }

    // Stereo-safe recent audio → mono chunk (keeps device rate)
    static AudioClip MakeRecentChunk(AudioClip rolling, float seconds, string deviceName)
    {
        if (rolling == null) return null;

        int rate = Mathf.Max(8000, rolling.frequency);
        int channels = Mathf.Max(1, rolling.channels);

        int needFrames = Mathf.Clamp(Mathf.RoundToInt(seconds * rate), 1, rolling.samples);
        int pos = Microphone.GetPosition(deviceName);
        if (pos < 0) return null;

        float[] interleaved = new float[needFrames * channels];

        int end = pos;               // frames
        int start = end - needFrames;

        if (start >= 0)
        {
            rolling.GetData(interleaved, start);
        }
        else
        {
            int firstFrames = -start;
            int secondFrames = needFrames - firstFrames;

            float[] tail = new float[firstFrames * channels];
            rolling.GetData(tail, rolling.samples + start);
            Array.Copy(tail, 0, interleaved, 0, tail.Length);

            float[] head = new float[secondFrames * channels];
            rolling.GetData(head, 0);
            Array.Copy(head, 0, interleaved, tail.Length, head.Length);
        }

        // Downmix to mono
        float[] mono = new float[needFrames];
        if (channels == 1)
            Array.Copy(interleaved, mono, mono.Length);
        else
        {
            for (int f = 0; f < needFrames; f++)
            {
                double sum = 0;
                int baseIdx = f * channels;
                for (int c = 0; c < channels; c++) sum += interleaved[baseIdx + c];
                mono[f] = (float)(sum / channels);
            }
        }

        var chunk = AudioClip.Create("stt_chunk", needFrames, 1, rate, false);
        chunk.SetData(mono, 0);
        return chunk;
    }

    IEnumerator TranscribeWithAzure(byte[] wavData, int sampleRateForHeader, Action<string> onResult)
    {
        if (string.IsNullOrEmpty(azureKey) || string.IsNullOrEmpty(azureRegion))
        {
            Debug.LogWarning("[STT][Azure] Missing key/region; skipping cloud call.");
            yield break;
        }

        string uri =
            $"https://{azureRegion}.stt.speech.microsoft.com/speech/recognition/dictation/cognitiveservices/v1?language=en-US" +
            (useDetailedFormat ? "&format=detailed" : "");

        using var req = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(wavData);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Ocp-Apim-Subscription-Key", azureKey);
        req.SetRequestHeader("Content-Type", $"audio/wav; codecs=audio/pcm; samplerate={sampleRateForHeader}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[STT][Azure] Error: {req.error}");
            yield break;
        }

        string json = req.downloadHandler.text;

        // Simple schema
        try
        {
            var simple = JsonUtility.FromJson<AzureSimple>(json);
            if (simple != null &&
                string.Equals(simple.RecognitionStatus, "Success", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(simple.DisplayText))
            {
                onResult?.Invoke(simple.DisplayText);
                yield break;
            }
        }
        catch { }

        // Detailed schema (NBest)
        if (useDetailedFormat)
        {
            try
            {
                var det = JsonUtility.FromJson<AzureDetailed>(json);
                if (det != null && det.NBest != null && det.NBest.Length > 0 && !string.IsNullOrEmpty(det.NBest[0].Display))
                {
                    onResult?.Invoke(det.NBest[0].Display);
                    yield break;
                }
            }
            catch { }
        }

        // Tolerant string pickers
        string display = TryExtractFirst(json, "\"DisplayText\":\"") ?? TryExtractFirst(json, "\"Display\":\"");
        if (!string.IsNullOrEmpty(display))
            onResult?.Invoke(display);
        else
            Debug.Log($"[STT][Azure] Unrecognized response:\n{json}");
    }

    static string TryExtractFirst(string json, string key)
    {
        int i = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return null;
        int start = i + key.Length;
        int end = json.IndexOf("\"", start, StringComparison.Ordinal);
        if (end < 0) return null;
        return json.Substring(start, end - start).Replace("\\n", "\n").Replace("\\\"", "\"");
    }

    static float ComputeRms(float[] samples)
    {
        double sum = 0;
        for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
        return Mathf.Sqrt((float)(sum / Math.Max(1, samples.Length)));
    }

    void TryLoadJson()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

#if UNITY_ANDROID || UNITY_WEBGL
            StartCoroutine(LoadJsonStreaming(path));
#else
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var cfg = JsonUtility.FromJson<AzureJson>(json);
                if (!string.IsNullOrEmpty(cfg.speechKey)) azureKey = cfg.speechKey;
                if (!string.IsNullOrEmpty(cfg.region))    azureRegion = cfg.region;
                Debug.Log("[STT] Loaded Azure config from StreamingAssets.");
            }
            else Debug.LogWarning($"[STT] JSON not found: {path}");
#endif
        }
        catch (Exception e) { Debug.LogWarning($"[STT] JSON load failed: {e.Message}"); }
    }

    IEnumerator LoadJsonStreaming(string url)
    {
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            var cfg = JsonUtility.FromJson<AzureJson>(req.downloadHandler.text);
            if (!string.IsNullOrEmpty(cfg.speechKey)) azureKey = cfg.speechKey;
            if (!string.IsNullOrEmpty(cfg.region)) azureRegion = cfg.region;
            Debug.Log("[STT] Loaded Azure config from StreamingAssets (UWR).");
        }
        else Debug.LogWarning($"[STT] JSON request failed: {req.error}");
    }

    // Utility to stop a coroutine safely if it's running
    void StopCoroutineSafe(IEnumerator routine)
    {
        // no-op helper; present for clarity if you refactor
    }
}
