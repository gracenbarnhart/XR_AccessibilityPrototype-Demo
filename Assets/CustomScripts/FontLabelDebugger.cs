using UnityEngine;

public class FontLabelDebugger : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("FontLabel Awake, activeInHierarchy = " + gameObject.activeInHierarchy);
    }
}
