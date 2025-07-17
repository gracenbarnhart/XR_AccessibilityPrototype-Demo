using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public GlassesSettings settings; // drag your GlassesSettings.asset here

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);

        // load from PlayerPrefs if present
        settings.isolateMode = PlayerPrefs.GetInt("isolateMode", 0) == 1;
        settings.isolatedSpeaker = PlayerPrefs.GetInt("isolatedSpeaker", 0);

        // ... you can also load other prefs here ...
    }

    /// <summary>
    /// Called by UI Toggle OnValueChanged
    /// </summary>
    public void SetIsolationMode(bool on)
    {
        settings.isolateMode = on;
        PlayerPrefs.SetInt("isolateMode", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Called by UI Dropdown OnValueChanged
    /// </summary>
    public void SetIsolatedSpeaker(int speakerId)
    {
        settings.isolatedSpeaker = speakerId;
        PlayerPrefs.SetInt("isolatedSpeaker", speakerId);
        PlayerPrefs.Save();
    }
}
