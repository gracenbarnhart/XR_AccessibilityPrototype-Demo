using UnityEngine;
using UnityEngine.XR;

public class EyeGazeManager : MonoBehaviour
{
    public static EyeGazeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    //true if raygaze valid
    public bool TryGetGaze(out Ray gazeRay)
    {
        gazeRay = new Ray();

        
        var device = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        if (device.isValid && device.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyes)
                          && eyes.TryGetFixationPoint(out Vector3 point))
        {
            Vector3 origin = Camera.main.transform.position;
            Vector3 direction = (point - origin).normalized;
            gazeRay = new Ray(origin, direction);
            return true;
        }

        return false;
    }
}

