using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SpeechToTextManager : MonoBehaviour
{
    public static SpeechToTextManager Instance { get; private set; }

    [Header("Config Source")]
    [Tooltip("Load Azure key & region from Assets/StreamingAssets/azure_speech.json")]
    public bool loadFromJson = true;
    public string jsonFileName = "azure_speech.json";

    [Header("Azure Speech Settings (fallback if JSON missing)")]
    [Tooltip("Azure Speech service key")]
    public string azureKey;
    [Tooltip("The region of Azure Speech resource (e.g. westus2, westeurope)")]
    public string azureRegion = "westeurope";

    [Header("Capture")]
    [Tooltip("Length (seconds) per chunk sent to Azure")]
    public float clipLength = 3f;
    [Tooltip("Mic sample rate (Azure expects 16000)")]
    public int sampleRate = 16000;
    [Tooltip("Rolling mic buffer length (seconds)")]
    public int rollingBufferSeconds = 10;

    public delegate void OnTranscription(string text, int speakerId);
    public event OnTranscription OnCaption;

    /// <summary>Simulates a caption for testing (press N)</summary>
    public void SimulateCaption(string txt, int id)
    {
        Debug.Log($"[STT] SimulateCaption: “{txt}” from speaker {id}");
        OnCaption?.Invoke(txt, id);
    }

    private AudioClip micClip;
    private string micDevice;

    [Serializable] private class AzureJson { public string speechKey; public string region; }
    [Serializable]
    private class AzureResponse // legacy simple schema
    {
        public string RecognitionStatus;
        public string DisplayText;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadFromJson) TryLoadJson();
    }

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[STT] No microphone found! Check Windows Privacy > Microphone.");
            enabled = false;
            return;
        }

        micDevice = Microphone.devices[0];

        // start rolling mic buffer
        micClip = Microphone.Start(micDevice, true, Mathf.Max(rollingBufferSeconds, 2), sampleRate);

        StartCoroutine(ContinuousTranscribe());
    }

    void Update()
    {
        // DEMO: press N to fake a caption without network
        if (Input.GetKeyDown(KeyCode.N))
            SimulateCaption("Demo caption (hotkey N).", 0);
    }

    IEnumerator ContinuousTranscribe()
    {
        // warm-up
        yield return new WaitForSeconds(1f);
        var wait = new WaitForSeconds(clipLength);

        while (true)
        {
            yield return wait;

            // extract ONLY the most recent clipLength seconds from the rolling buffer
            var chunk = MakeRecentChunk(micClip, clipLength, sampleRate, micDevice);
            if (chunk == null) continue;

            // encode to WAV using your WavUtility (no file saved)
            string _;
            byte[] wav = WavUtility.FromAudioClip(chunk, out _, saveAsFile: false);

            // send to Azure
            yield return StartCoroutine(TranscribeWithAzure(wav, text =>
            {
                if (!string.IsNullOrEmpty(text))
                    OnCaption?.Invoke(text, 0);
            }));
        }
    }

    static AudioClip MakeRecentChunk(AudioClip rolling, float seconds, int sampleRate, string device)
    {
        if (rolling == null) return null;

        int need = Mathf.Clamp(Mathf.RoundToInt(seconds * sampleRate), 1, rolling.samples);
        int micPos = Microphone.GetPosition(device);
        if (micPos < 0) return null;

        float[] buffer = new float[need];

        int end = micPos;
        int start = end - need;

        if (start >= 0)
        {
            rolling.GetData(buffer, start);
        }
        else
        {
            int firstLen = -start;
            float[] head = new float[firstLen];
            float[] tail = new float[need - firstLen];

            rolling.GetData(head, rolling.samples + start);
            rolling.GetData(tail, 0);

            Array.Copy(head, 0, buffer, 0, head.Length);
            Array.Copy(tail, 0, buffer, head.Length, tail.Length);
        }

        var chunk = AudioClip.Create("stt_chunk", need, 1, sampleRate, false);
        chunk.SetData(buffer, 0);
        return chunk;
    }

    IEnumerator TranscribeWithAzure(byte[] wavData, Action<string> onResult)
    {
        if (string.IsNullOrEmpty(azureKey) || string.IsNullOrEmpty(azureRegion))
        {
            Debug.LogWarning("[STT][Azure] Missing key/region; skipping cloud call.");
            yield break;
        }

        var uri = $"https://{azureRegion}.stt.speech.microsoft.com/speech/recognition/" +
                  "conversation/cognitiveservices/v1?language=en-US";

        using var req = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(wavData);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Ocp-Apim-Subscription-Key", azureKey);
        req.SetRequestHeader("Content-Type", "audio/wav; codecs=audio/pcm; samplerate=16000");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[STT][Azure] Error: {req.error}");
            yield break;
        }

        string json = req.downloadHandler.text;

        // Try simple schema first
        try
        {
            var parsed = JsonUtility.FromJson<AzureResponse>(json);
            if (parsed != null && parsed.RecognitionStatus == "Success" && !string.IsNullOrEmpty(parsed.DisplayText))
            {
                onResult?.Invoke(parsed.DisplayText);
                yield break;
            }
        }
        catch { /* fall through to tolerant parser */ }

        // Fallback: tolerant grab of "DisplayText"
        string display = TryExtractDisplayText(json);
        if (!string.IsNullOrEmpty(display))
            onResult?.Invoke(display);
        else
            Debug.Log($"[STT][Azure] Unrecognized response:\n{json}");
    }

    static string TryExtractDisplayText(string json)
    {
        const string key = "\"DisplayText\":\"";
        int i = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return null;
        int start = i + key.Length;
        int end = json.IndexOf("\"", start, StringComparison.Ordinal);
        if (end < 0) return null;
        return json.Substring(start, end - start).Replace("\\n", "\n").Replace("\\\"", "\"");
    }

    void TryLoadJson()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

#if UNITY_ANDROID || UNITY_WEBGL
            // If you ever target these, StreamingAssets needs UWR:
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
}
