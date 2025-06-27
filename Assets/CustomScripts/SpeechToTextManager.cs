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

    void Update()
    {
        // 🔁 Press N to simulate a caption from speaker 0
        if (Input.GetKeyDown(KeyCode.N))
        {
            SimulateCaption("Hello!", 0);
        }
    }

    // 🚨 Replace this with real STT later
    public void SimulateCaption(string txt, int id)
    {
        Debug.Log($"[STT] SimulateCaption: “{txt}” from speaker {id}");
        OnCaption?.Invoke(txt, id);
    }
}
