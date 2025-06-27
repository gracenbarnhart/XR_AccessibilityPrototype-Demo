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

    // Keep track of the running hide coroutine
    private Coroutine hideRoutine;

    void Update()
    {
        // 1) Check your sound‐source keys
        foreach (var src in sources)
            if (Input.GetKeyDown(src.triggerKey))
                ShowEvent(src);

        // 2) Simulate a caption with 'N' key
        if (Input.GetKeyDown(KeyCode.N))
            SpeechToTextManager.Instance.SimulateCaption("Hello world", 0);
    }

    void OnEnable()
    {
        // When this script becomes active, start listening
        SpeechToTextManager.Instance.OnCaption += HandleCaption;
    }

    void OnDisable()
    {
        // Stop listening when disabled (avoid memory leaks)
        SpeechToTextManager.Instance.OnCaption -= HandleCaption;
    }

    // Called when SpeechToTextManager.SimulateCaption(...) fires
    void HandleCaption(string text, int speakerId)
    {
        Debug.Log($"[HUD] HandleCaption received: “{text}” (Speaker {speakerId})");
        captionLabel.text = $"Speaker {speakerId}: {text}";
        worldSpaceHUD.gameObject.SetActive(true);

        // Position HUD in front of the camera again
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // Reset HUD hide timer
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));
    }

    void ShowEvent(AcousticSource src)
    {
        // 1) Spawn the 3D marker
        var marker = Instantiate(hudMarkerPrefab);
        marker.transform.position = src.worldPosition;

        // 2) Configure & show the 2D HUD
        worldSpaceHUD.gameObject.SetActive(true);
        captionLabel.enableWordWrapping = false;
        captionLabel.overflowMode = TextOverflowModes.Overflow;
        captionLabel.alignment = TextAlignmentOptions.Center;
        captionLabel.text = "🔊 " + src.name;

        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // 3) Cancel previous hide, start new timer
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));

        // 4) Haptics
        HapticManager.Instance.TriggerHaptic(XRNode.RightHand);
    }

    IEnumerator HideHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        worldSpaceHUD.gameObject.SetActive(false);
        hideRoutine = null;
    }
}
