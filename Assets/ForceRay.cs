using UnityEngine;

public class ForceRayForward : MonoBehaviour
{
    public Vector3 localPos = new Vector3(0f, 0f, 0.08f);
    public Vector3 localEuler = new Vector3(-90f, 0f, 0f); // 
    void LateUpdate()
    {
        transform.localPosition = localPos;
        transform.localRotation = Quaternion.Euler(localEuler);
    }
}
