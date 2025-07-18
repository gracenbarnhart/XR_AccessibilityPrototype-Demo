using UnityEngine;

public class UIFollow : MonoBehaviour
{
    [Tooltip("The transform to follow (usually Camera.main)")]
    public Transform target;
    [Tooltip("Local offset relative to the camera")]
    public Vector3 offset = new Vector3(0f, -0.2f, 2f);

    void LateUpdate()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target != null)
        {
            // position in front of the user
            transform.position = target.position
                               + target.right * offset.x
                               + target.up * offset.y
                               + target.forward * offset.z;

            // face the user
            transform.rotation = Quaternion.LookRotation(
                transform.position - target.position,
                Vector3.up
            );
        }
    }
}
