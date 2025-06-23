using UnityEngine;

public class HUDFadeOut : MonoBehaviour
{
    [Tooltip("How long (in seconds) the HUD marker remains visible")]
    public float displayTime = 2f;

    void Start()
    {
        // Destroy this GameObject after displayTime seconds
        Destroy(gameObject, displayTime);
    }

    void Update()
    {
        // Always face the main camera so the HUD “billboards”
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}
