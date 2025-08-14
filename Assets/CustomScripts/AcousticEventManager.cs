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
    [Tooltip("Drag HUD_Marker prefab here")]
    public GameObject hudMarkerPrefab;

    [Header("2D World-Space HUD")]
    [Tooltip("Drag World-Space Canvas here")]
    public Canvas worldSpaceHUD;
    [Tooltip("Drag TextMeshProUGUI caption here")]
    public TextMeshProUGUI captionLabel;

    [Header("Naming UI")]
    [Tooltip("Drag existing NamePanel (with InputField + Save) here")]
    public GameObject namePanel;

    [Header("HUD Behaviour")]
    [Tooltip("If true, HUD is forced to stay visible and is never deactivated")]
    public bool alwaysShowHUD = true;
    [Tooltip("Only used if alwaysShowHUD == false. Seconds the HUD stays visible before hiding.")]
    public float hudDisplayTime = 5f;

    [Header("Known Speaker Sources")]
    [Tooltip("Define each speaker's world position here")]
    public AcousticSource[] sources;

    private Coroutine hideRoutine;

    void Start()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption += HandleCaption;
        else
            Debug.LogError("[AcousticEventManager] No SpeechToTextManager found!");

        if (worldSpaceHUD != null && alwaysShowHUD)
            worldSpaceHUD.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption -= HandleCaption;
    }

    private void HandleCaption(string text, int speakerId)
    {
        var s = SettingsManager.Instance.settings;

        if (s.isolateMode && speakerId != s.isolatedSpeaker)
            return;

        string key = $"speakerName_{speakerId}";
        if (!PlayerPrefs.HasKey(key))
        {
            var setter = namePanel.GetComponent<SpeakerNameSetter>();
            setter.speakerId = speakerId;
            namePanel.SetActive(true);
            return;
        }

        if (speakerId >= 0 && speakerId < sources.Length && hudMarkerPrefab != null)
        {
            var marker = Instantiate(hudMarkerPrefab);
            marker.transform.position = sources[speakerId].worldPosition;
        }

        string displayName = SpeakerManager.Instance.GetName(speakerId);
        if (captionLabel != null)
        {
            captionLabel.text = $"🔊 {displayName}: {text}";
            captionLabel.color = s.captionColor;
            captionLabel.fontSize = s.fontSize;
        }

        if (worldSpaceHUD != null)
        {
            worldSpaceHUD.gameObject.SetActive(true);
            PositionHUDInFront();

            if (alwaysShowHUD)
            {
                if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
            }
            else
            {
                float delay = Mathf.Max(1.0f, hudDisplayTime); 
                if (hideRoutine != null) StopCoroutine(hideRoutine);
                hideRoutine = StartCoroutine(HideHUDDelayed(delay));
            }
        }

        if (HapticManager.Instance != null)
            HapticManager.Instance.TriggerHaptic(XRNode.RightHand);
    }

    private IEnumerator HideHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (worldSpaceHUD != null)
            worldSpaceHUD.gameObject.SetActive(false);
        hideRoutine = null;
    }

    private void PositionHUDInFront()
    {
        var cam = Camera.main?.transform;
        if (cam == null || worldSpaceHUD == null) return;

        const float distance = 2f;
        Vector3 basePos = cam.position + cam.forward * distance;

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

        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );
    }
}
