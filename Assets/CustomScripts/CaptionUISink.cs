using TMPro;
using UnityEngine;

public class CaptionUISink : MonoBehaviour
{
    [SerializeField] TMP_Text captionText;

    void OnEnable()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption += HandleCaption;
    }

    void OnDisable()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption -= HandleCaption;
    }

    void HandleCaption(string text, int speakerId)
    {
        if (captionText != null)
            captionText.text = text;
    }
}
