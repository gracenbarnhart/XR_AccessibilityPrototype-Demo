using UnityEngine;

public class HUDFadeOut : MonoBehaviour
{
    [Tooltip("How long (in seconds) the HUD marker remains visible")]
    public float displayTime = 2f;

    void Start()
    {
        
        Destroy(gameObject, displayTime);
    }

    void Update()
    {
        
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}
