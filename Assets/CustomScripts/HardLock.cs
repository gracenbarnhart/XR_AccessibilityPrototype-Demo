using UnityEngine;

[DefaultExecutionOrder(10000)]  
public class HardLockToCamera : MonoBehaviour
{
    public Transform cam;                
    public Vector3 localPosition = new Vector3(0f, 0f, 1.2f);
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = new Vector3(0.001f, 0.001f, 0.001f);

    Rigidbody rb;

    void Awake()
    {
        if (!cam) cam = Camera.main ? Camera.main.transform : null;
        if (!cam) return;

        transform.SetParent(cam, false);

        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Apply();
    }

    void LateUpdate()
    {
        if (!cam) { cam = Camera.main ? Camera.main.transform : null; if (!cam) return; }
        Apply();

        // keep physics frozen if a RB appears
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Apply()
    {
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(localEuler);
        transform.localScale = localScale;
    }
}
