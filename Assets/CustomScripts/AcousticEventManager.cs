using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System.Collections;

public class AcousticEventManager : MonoBehaviour
{
    [System.Serializable]
    public class AcousticSource
    {
        public string name;
        public Vector3 worldPosition;
    }

    [Header("3D Marker")]
    [Tooltip("Drag your HUD_Marker prefab here")]
    public GameObject hudMarkerPrefab;

    [Header("2D World-Space HUD")]
    [Tooltip("Drag your World-Space Canvas here")]
    public Canvas worldSpaceHUD;
    [Tooltip("Drag your TextMeshProUGUI caption here")]
    public TextMeshProUGUI captionLabel;

    [Header("Naming UI")]
    [Tooltip("Drag your existing NamePanel (with InputField + Save) here")]
    public GameObject namePanel;

    [Header("Timing")]
    [Tooltip("Seconds the HUD stays visible")]
    public float hudDisplayTime = 5f;

    [Header("Known Speaker Sources")]
    [Tooltip("Define each speaker's world position here")]
    public AcousticSource[] sources;

    // tracks the hide coroutine so we can restart it
    private Coroutine hideRoutine;

    void Start()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption += HandleCaption;
        else
            Debug.LogError("[AcousticEventManager] No SpeechToTextManager found!");
    }

    void OnDisable()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption -= HandleCaption;
    }

    private void HandleCaption(string text, int speakerId)
    {
        var s = SettingsManager.Instance.settings;

        // 1) Isolation filter
        if (s.isolateMode && speakerId != s.isolatedSpeaker)
            return;

        // 2) Dynamic naming popup
        string key = $"speakerName_{speakerId}";
        if (!PlayerPrefs.HasKey(key))
        {
            var setter = namePanel.GetComponent<SpeakerNameSetter>();
            setter.speakerId = speakerId;
            namePanel.SetActive(true);
            return;
        }

        // 3) Spawn 3D marker at that speaker’s worldPosition
        if (speakerId >= 0 && speakerId < sources.Length)
        {
            var marker = Instantiate(hudMarkerPrefab);
            marker.transform.position = sources[speakerId].worldPosition;
        }

        // 4) Update caption text, color & size
        string displayName = SpeakerManager.Instance.GetName(speakerId);
        captionLabel.text = $"🔊 {displayName}: {text}";
        captionLabel.color = s.captionColor;
        captionLabel.fontSize = s.fontSize;

        // 5) Show & position the HUD in front of camera, offset by dropdown
        worldSpaceHUD.gameObject.SetActive(true);
        PositionHUDInFront();

        // 6) Restart hide timer
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));

        // 7) Haptic feedback
        HapticManager.Instance.TriggerHaptic(XRNode.RightHand);
    }

    private IEnumerator HideHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        worldSpaceHUD.gameObject.SetActive(false);
        hideRoutine = null;
    }

    /// <summary>
    /// Positions the world-space HUD two meters ahead of the camera,
    /// then offsets it based on the user's TextPosition setting.
    /// </summary>
    private void PositionHUDInFront()
    {
        var cam = Camera.main.transform;
        const float distance = 2f;
        Vector3 basePos = cam.position + cam.forward * distance;

        // tweak offsets to taste
        const float hOffset = 0.5f;
        const float vOffset = 0.4f;

        switch (SettingsManager.Instance.settings.textPosition)
        {
            case TextPosition.TopLeft:
                worldSpaceHUD.transform.position = basePos + cam.up * vOffset - cam.right * hOffset;
                break;
            case TextPosition.TopRight:
                worldSpaceHUD.transform.position = basePos + cam.up * vOffset + cam.right * hOffset;
                break;
            case TextPosition.BottomLeft:
                worldSpaceHUD.transform.position = basePos - cam.up * vOffset - cam.right * hOffset;
                break;
            case TextPosition.BottomRight:
                worldSpaceHUD.transform.position = basePos - cam.up * vOffset + cam.right * hOffset;
                break;
            case TextPosition.Center:
            default:
                worldSpaceHUD.transform.position = basePos;
                break;
        }

        // Always face the camera
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );
    }
}
