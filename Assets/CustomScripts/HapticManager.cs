using UnityEngine;
using UnityEngine.XR;

public class HapticManager : MonoBehaviour
{
    // Singleton instance
    public static HapticManager Instance { get; private set; }

    [Tooltip("Vibration strength (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float amplitude = 0.75f;

    [Tooltip("Time in seconds the vibration lasts")]
    public float duration = 0.15f;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Trigger a single haptic impulse on the given hand/controller.
    /// </summary>
    public void TriggerHaptic(XRNode handNode)
    {
        var device = InputDevices.GetDeviceAtXRNode(handNode);
        if (device.isValid &&
            device.TryGetHapticCapabilities(out HapticCapabilities caps) &&
            caps.supportsImpulse)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
}
