using UnityEngine;
using UnityEngine.XR;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }
    [Range(0f, 1f)] public float amplitude = .75f;
    public float duration = .15f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerHaptic(XRNode node)
    {
        Debug.Log($"[HAPTIC] Triggering on {node}");
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (device.isValid && device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            device.SendHapticImpulse(0, amplitude, duration);
            Debug.Log($"[HAPTIC] Sent impulse – amplitude: {amplitude}, duration: {duration}");
        }
        else
        {
            Debug.LogWarning($"[HAPTIC] No haptic support on {node} (isValid: {device.isValid})");
        }
    }
}
