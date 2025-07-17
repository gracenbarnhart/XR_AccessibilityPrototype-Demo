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
        public KeyCode triggerKey;
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
    [Tooltip("Drag your existing NamePanel (the one with InputField + Save button) here")]
    public GameObject namePanel;

    [Header("Timing")]
    [Tooltip("Seconds the 2D HUD stays visible")]
    public float hudDisplayTime = 5f;

    [Header("Sources")]
    [Tooltip("Define your sound sources here")]
    public AcousticSource[] sources;

    // Tracks the running hide coroutine so we can restart it
    private Coroutine hideRoutine;

    // Subscribe in Start() to avoid race conditions
    void Start()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption += HandleCaption;
        else
            Debug.LogError("[AcousticEventManager] No SpeechToTextManager found!");
    }

    void OnEnable()
    {
        // Already hooked in Start()
    }

    void OnDisable()
    {
        if (SpeechToTextManager.Instance != null)
            SpeechToTextManager.Instance.OnCaption -= HandleCaption;
    }

    void Update()
    {
        // 1) Fire off 3D + 2D HUD when keys pressed
        foreach (var src in sources)
            if (Input.GetKeyDown(src.triggerKey))
                ShowEvent(src);

        // 2) (optional) Eye-gaze driven speaker focus
        if (EyeGazeManager.Instance.TryGetGaze(out Ray gazeRay))
        {
            foreach (var src in sources)
                if (Physics.Raycast(gazeRay, out RaycastHit hit) &&
                    hit.collider.gameObject.name == src.name)
                    ShowEvent(src);
        }
    }

    void HandleCaption(string text, int speakerId)
    {
        // ─────────────────────────────────────────────────────────────
        // 1) Speaker Isolation Filter
        var s = SettingsManager.Instance.settings;
        if (s.isolateMode && speakerId != s.isolatedSpeaker)
            return;
        // ─────────────────────────────────────────────────────────────

        // ─────────────────────────────────────────────────────────────
        // 2) Dynamic Naming Popup
        string key = $"speakerName_{speakerId}";
        if (!PlayerPrefs.HasKey(key))
        {
            // Make sure your NamePanel has a SpeakerNameSetter component
            var setter = namePanel.GetComponent<SpeakerNameSetter>();
            setter.speakerId = speakerId;
            namePanel.SetActive(true);
            return;
        }
        // ─────────────────────────────────────────────────────────────

        // 3) Look up the saved name
        string displayName = SpeakerManager.Instance.GetName(speakerId);

        // 4) Apply configured text color & size
        captionLabel.color = s.captionColor;
        captionLabel.fontSize = s.fontSize;

        // 5) Update the caption text
        captionLabel.text = $"🔊 {displayName}: {text}";

        // 6) Show the HUD
        worldSpaceHUD.gameObject.SetActive(true);

        // 7) Position & orient in front of camera
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // 8) Restart hide‐timer
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));
    }

    void ShowEvent(AcousticSource src)
    {
        // Spawn 3D marker
        var marker = Instantiate(hudMarkerPrefab);
        marker.transform.position = src.worldPosition;

        // Show 2D HUD
        worldSpaceHUD.gameObject.SetActive(true);

        // Apply color & size
        var settings = SettingsManager.Instance.settings;
        captionLabel.color = settings.captionColor;
        captionLabel.fontSize = settings.fontSize;

        captionLabel.enableWordWrapping = false;
        captionLabel.overflowMode = TextOverflowModes.Overflow;
        captionLabel.alignment = TextAlignmentOptions.Center;
        captionLabel.text = "🔊 " + src.name;

        // Position & orient
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // Restart hide‐timer
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));

        // Haptic feedback
        HapticManager.Instance.TriggerHaptic(XRNode.RightHand);
    }

    IEnumerator HideHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        worldSpaceHUD.gameObject.SetActive(false);
        hideRoutine = null;
    }
}
