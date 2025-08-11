using UnityEngine;

public class HUDFadeOut : MonoBehaviour
{
    public float displayTime = 2f; // can be ignored if we don't destroy it

    void Start()
    {
        // Destroy(gameObject, displayTime); // disable auto-delete
    }

    void Update()
    {
        // Optional: Keep facing camera
        // transform.LookAt(Camera.main.transform);
    }
}
