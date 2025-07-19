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

        // 2) Dynamic naming
        string key = $"speakerName_{speakerId}";
        if (!PlayerPrefs.HasKey(key))
        {
            var setter = namePanel.GetComponent<SpeakerNameSetter>();
            setter.speakerId = speakerId;
            namePanel.SetActive(true);
            return;
        }

        // 3) Spawn 3D marker
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

        // 5) Show & position HUD 2m in front of camera
        worldSpaceHUD.gameObject.SetActive(true);
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

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
}
