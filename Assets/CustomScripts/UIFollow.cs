using UnityEngine;

public class UIFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, -0.2f, 2f);
    public bool followContinuously = false;   

    void Start() { if (!target) target = Camera.main ? Camera.main.transform : null; Place(); if (!followContinuously) enabled = false; }
    void LateUpdate() { Place(); }

    void Place()
    {
        if (!target) return;
        transform.position = target.position + target.right * offset.x + target.up * offset.y + target.forward * offset.z;
        transform.rotation = Quaternion.LookRotation(transform.position - target.position, Vector3.up);
    }
}
