using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeakerNameSetter : MonoBehaviour
{
    [Tooltip("Drag your TMP InputField here")]
    public TMP_InputField nameInput;

    [Tooltip("Which speaker ID this panel is naming")]
    public int speakerId = 0;

    [Tooltip("Drag the NamePanel GameObject here to hide it after saving")]
    public GameObject namePanel;

    // Called by the SaveNameBtn OnClick()
    public void OnSaveName()
    {
        var name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        // 1) Save into SpeakerManager + PlayerPrefs
        SpeakerManager.Instance.SetName(speakerId, name);

        // 2) Hide the panel
        namePanel.SetActive(false);
    }
}
