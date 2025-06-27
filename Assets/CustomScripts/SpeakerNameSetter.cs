using UnityEngine;
using TMPro;

public class SpeakerNameSetter : MonoBehaviour
{
    public TMP_InputField inputField;

    public void SetSpeakerZeroName()
    {
        string name = inputField.text;
        Debug.Log($"[UI] Saving speaker 0 name = \"{name}\"");
        SpeakerManager.Instance.SetName(0, name);
    }
}
