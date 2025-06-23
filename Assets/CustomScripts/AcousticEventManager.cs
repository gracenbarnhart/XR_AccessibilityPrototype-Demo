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

    // keep track of the running hide coroutine
    private Coroutine hideRoutine;

    void Update()
    {
        foreach (var src in sources)
        {
            if (Input.GetKeyDown(src.triggerKey))
                ShowEvent(src);
        }
    }

    void ShowEvent(AcousticSource src)
    {
        // 1) Spawn the 3D marker as before
        var marker = Instantiate(hudMarkerPrefab);
        marker.transform.position = src.worldPosition;

        // 2) Configure & show the 2D HUD
        captionLabel.enableWordWrapping = false;                   // disable wrapping
        captionLabel.overflowMode = TextOverflowModes.Overflow; // no clipping
        captionLabel.alignment = TextAlignmentOptions.Center;
        captionLabel.text = "🔊 " + src.name;

        worldSpaceHUD.gameObject.SetActive(true);

        // position it 2m in front of the camera
        var cam = Camera.main.transform;
        worldSpaceHUD.transform.position = cam.position + cam.forward * 2f;
        worldSpaceHUD.transform.rotation = Quaternion.LookRotation(
            worldSpaceHUD.transform.position - cam.position
        );

        // 3) Cancel any previous hide, then start a fresh one
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideHUDDelayed(hudDisplayTime));
    }

    IEnumerator HideHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        worldSpaceHUD.gameObject.SetActive(false);
        hideRoutine = null;
    }
}
