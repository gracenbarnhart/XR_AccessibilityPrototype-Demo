using UnityEngine;
public class SpeechToTextManager : MonoBehaviour
{
    public static SpeechToTextManager Instance { get; private set; }
    public delegate void OnTranscription(string text, int speakerId);
    public event OnTranscription OnCaption;

    void Awake()
    {
        Instance = this;
        Debug.Log("[STT] SpeechToTextManager initialized");
    }

    // TODO: integrate real STT here.
    public void SimulateCaption(string txt, int id)
    {
        Debug.Log($"[STT] SimulateCaption: “{txt}” from speaker {id}");
        OnCaption?.Invoke(txt, id);
    }
}
