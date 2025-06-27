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

    [Header("Timing")]
    [Tooltip("Seconds the 2D HUD stays visible")]
    public float hudDisplayTime = 5f;

    [Header("Sources")]
    [Tooltip("Define your sound sources here")]
    public AcousticSource[] sources;

    // Tracks the running hide coroutine so we can restart it
    private Coroutine hideRoutine;

    void OnEnable()
    {
        // Subscribe to incoming captions
        SpeechToTextManager.Instance.OnCaption += HandleCaption;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        SpeechToTextManager.Instance.OnCaption -= HandleCaption;
    }

    void Update()
    {
        // 1) Fire off 3D + 2D HUD when keys pressed
        foreach (var src in sources)
            if (Input.GetKeyDown(src.triggerKey))
                ShowEvent(src);

        // 2) For testing: press N to simulate a caption from speaker 0
        if (Input.GetKeyDown(KeyCode.N))
            SpeechToTextManager.Instance.SimulateCaption("Hello world", 0);
    }

    void HandleCaption(string text, int speakerId)
    {
        // 1) Look up the saved name (defaults to "Speaker X")
        string displayName = SpeakerManager.Instance.GetName(speakerId);

        // 2) Update the caption text on the HUD
        captionLabel.text = $"🔊 {displayName}: {text}";

        // 3) Ensure the HUD panel is visible
        worldSpaceHUD.gameObject.SetActive(true);

        // 4) Re-position & orient the HUD in front of the camera
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // 5) Restart the hide‐timer
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));
    }

    void ShowEvent(AcousticSource src)
    {
        // Spawn the 3D marker at the source's world position
        var marker = Instantiate(hudMarkerPrefab);
        marker.transform.position = src.worldPosition;

        // Configure & show the 2D HUD with the raw source name
        worldSpaceHUD.gameObject.SetActive(true);
        captionLabel.enableWordWrapping = false;
        captionLabel.overflowMode = TextOverflowModes.Overflow;
        captionLabel.alignment = TextAlignmentOptions.Center;
        captionLabel.text = "🔊 " + src.name;

        // Position & orient the HUD
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // Restart hide-timer
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));

        // Trigger haptic feedback on the right hand
        HapticManager.Instance.TriggerHaptic(XRNode.RightHand);
    }

    IEnumerator HideHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        worldSpaceHUD.gameObject.SetActive(false);
        hideRoutine = null;
    }
}
