using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SpeechToTextManager : MonoBehaviour
{
    public static SpeechToTextManager Instance { get; private set; }

    [Header("Azure Speech Settings")]
    [Tooltip("Azure Speech service key")]
    public string azureKey;
    [Tooltip("The region of Azure Speech resource (e.g. westus2, westeurope)")]
    public string azureRegion;

    [Tooltip("Length in seconds of each audio clip sent for transcription")]
    public float clipLength = 3f;

    public delegate void OnTranscription(string text, int speakerId);
    public event OnTranscription OnCaption;

    // add the below stub **here**, directly after event:
    /// <summary>
    /// simulates a caption for testing (press N in Update())
    /// </summary>
    public void SimulateCaption(string txt, int id)
    {
        Debug.Log($"[STT] SimulateCaption: “{txt}” from speaker {id}");
        OnCaption?.Invoke(txt, id);
    }

    private AudioClip micClip;
    private string micDevice;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[STT] No microphone found!");
            return;
        }
        micDevice = Microphone.devices[0];
        //  an ever-rolling buffer of 10s at 16kHz
        micClip = Microphone.Start(micDevice, true, 10, 16000);
        StartCoroutine(ContinuousTranscribe());
    }

    IEnumerator ContinuousTranscribe()
    {
        // warm-up
        yield return new WaitForSeconds(1f);

        var wait = new WaitForSeconds(clipLength);

        while (true)
        {
            yield return wait;

            // convert the entire micClip to WAV
            byte[] wav = WavUtility.FromAudioClip(micClip);

            // send to Azure
            yield return StartCoroutine(TranscribeWithAzure(wav, text =>
            {
                OnCaption?.Invoke(text, 0);
            }));
        }
    }

    IEnumerator TranscribeWithAzure(byte[] wavData, Action<string> onResult)
    {
        var uri = $"https://{azureRegion}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=en-US";
        using var req = new UnityWebRequest(uri, "POST");
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

        try
        {
            var json = req.downloadHandler.text;
            var result = JsonUtility.FromJson<AzureResponse>(json);
            if (result.RecognitionStatus == "Success")
                onResult(result.DisplayText);
        }
        catch (Exception e)
        {
            Debug.LogError($"[STT][Azure] JSON parse error: {e}");
        }
    }

    [Serializable]
    private class AzureResponse
    {
        public string RecognitionStatus;
        public string DisplayText;
    }
}
