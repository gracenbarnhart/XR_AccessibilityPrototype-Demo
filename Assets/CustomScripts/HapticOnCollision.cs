using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Collider))]
public class HapticOnCollision : MonoBehaviour
{
    public XRNode handToTrigger = XRNode.RightHand;
    void OnCollisionEnter(Collision c)
    {
        if (c.collider.CompareTag("XRController"))
            HapticManager.Instance.TriggerHaptic(handToTrigger);
    }
}

