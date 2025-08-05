using UnityEngine;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public GlassesSettings settings; 

    // whenever the textPosition value changes
    public event Action<TextPosition> OnTextPositionChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);

        // load existing prefs
        settings.isolateMode = PlayerPrefs.GetInt("isolateMode", 0) == 1;
        settings.isolatedSpeaker = PlayerPrefs.GetInt("isolatedSpeaker", 0);
        settings.fontSize = PlayerPrefs.GetInt("fontSize", settings.fontSize);
        settings.captionColor = LoadColor("captionColor", settings.captionColor);
        settings.textPosition = (TextPosition)PlayerPrefs.GetInt("textPos", (int)settings.textPosition);
    }

    Color LoadColor(string key, Color fallback)
    {
        var s = PlayerPrefs.GetString(key, null);
        if (string.IsNullOrEmpty(s)) return fallback;
        var p = s.Split(',');
        return new Color(
            float.Parse(p[0]),
            float.Parse(p[1]),
            float.Parse(p[2]),
            float.Parse(p[3])
        );
    }

    void SaveColor(string key, Color c)
    {
        PlayerPrefs.SetString(key, $"{c.r},{c.g},{c.b},{c.a}");
        PlayerPrefs.Save();
    }

    //isolation methods
    public void SetIsolationMode(bool on)
    {
        settings.isolateMode = on;
        PlayerPrefs.SetInt("isolateMode", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetIsolatedSpeaker(int speakerId)
    {
        settings.isolatedSpeaker = speakerId;
        PlayerPrefs.SetInt("isolatedSpeaker", speakerId);
        PlayerPrefs.Save();
    }

    // called by fontSizeSlider.onValueChanged
    public void SetFontSize(float size)
    {
        settings.fontSize = Mathf.RoundToInt(size);
        PlayerPrefs.SetInt("fontSize", settings.fontSize);
        PlayerPrefs.Save();
    }

    // called by colorDropdown.onValueChanged
    public void SetCaptionColor(int idx)
    {
        var colors = new Color[] { Color.white, Color.yellow, Color.cyan, Color.green };
        settings.captionColor = colors[Mathf.Clamp(idx, 0, colors.Length - 1)];
        SaveColor("captionColor", settings.captionColor);
    }

    // called by positionDropdown.onValueChanged
    public void SetTextPosition(int idx)
    {
        settings.textPosition = (TextPosition)idx;
        PlayerPrefs.SetInt("textPos", idx);
        PlayerPrefs.Save();

        // notify any listeners (e.g. CaptionPositioner)
        OnTextPositionChanged?.Invoke(settings.textPosition);
    }
}
