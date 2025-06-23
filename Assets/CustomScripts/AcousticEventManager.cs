using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class AcousticEventManager : MonoBehaviour
{
    [System.Serializable]
    public class AcousticSource
    {
        public string name;           // e.g. "Doorbell"
        public KeyCode triggerKey;    // e.g. KeyCode.Alpha1
        public Vector3 worldOffset;   // offset *relative* to the XR Origin
    }

    [Tooltip("Drag your XR Origin (the root of your rig) here")]
    public Transform xrOrigin;

    [Tooltip("Offsets relative to XR Origin")]
    public AcousticSource[] sources;

    [Tooltip("Drag your HUD_Marker prefab here")]
    public GameObject hudMarkerPrefab;

    [Tooltip("Uniform scale applied to each spawned marker")]
    [Range(0.1f, 3f)]
    public float markerScale = 1f;

    [Tooltip("Seconds before the marker auto-destroys")]
    public float displayTime = 5f;

    void Update()
    {
        foreach (var src in sources)
        {
            if (Input.GetKeyDown(src.triggerKey))
            {
                // compute spawn position
                Vector3 spawnPos = xrOrigin.position + src.worldOffset;

                // instantiate & scale
                var marker = Instantiate(hudMarkerPrefab, spawnPos, Quaternion.identity);
                marker.transform.localScale = Vector3.one * markerScale;

                // face the camera
                var cam = Camera.main.transform;
                Vector3 dir = (cam.position - marker.transform.position).normalized;
                marker.transform.rotation = Quaternion.LookRotation(dir);

                // set the text
                var tmp = marker.GetComponentInChildren<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = src.name;
                    // flip the text mesh so it reads correctly
                    tmp.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }

                // haptic pulse
                HapticManager.Instance.TriggerHaptic(XRNode.RightHand);

                // auto-destroy
                Destroy(marker, displayTime);
            }
        }
    }
}
