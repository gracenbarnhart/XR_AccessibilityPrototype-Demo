using UnityEngine;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public GlassesSettings settings;        // drag your asset here in inspector
    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
        // Load PlayerPrefs overrides (step 5 will fill this out)
    }
}
