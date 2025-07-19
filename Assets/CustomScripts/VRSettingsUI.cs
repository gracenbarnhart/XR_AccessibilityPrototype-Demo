using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRSettingsUI : MonoBehaviour
{
    [Header("Collapse")]
    public Button collapseButton;

    [Header("Isolation")]
    public Toggle isolateToggle;
    public TMP_Dropdown speakerDropdown;

    [Header("Caption Style")]
    public Slider fontSizeSlider;
    public TMP_Dropdown colorDropdown;
    public TMP_Dropdown positionDropdown;

    [Header("Noise Display Wiring")]
    public Image warningIcon;
    public RawImage spectrogramView;

    private GlassesSettings S => SettingsManager.Instance.settings;

    void Start()
    {
        // 1) Collapse/Expand panel children
        if (collapseButton != null)
            collapseButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var go = transform.GetChild(i).gameObject;
                    if (go != collapseButton.gameObject)
                        go.SetActive(!go.activeSelf);
                }
            });

        // 2) Isolation toggle
        isolateToggle.isOn = S.isolateMode;
        isolateToggle.onValueChanged.AddListener(SettingsManager.Instance.SetIsolationMode);

        // 3) Speaker dropdown from SpeakerManager
        speakerDropdown.ClearOptions();
        var saved = SpeakerManager.Instance.GetAllNames();
        var ids = new List<int>(saved.Keys);
        var disp = new List<string>(saved.Values);
        speakerDropdown.AddOptions(disp);

        int currId = S.isolatedSpeaker;
        int idx = ids.IndexOf(currId);
        speakerDropdown.value = (idx >= 0 ? idx : 0);
        speakerDropdown.onValueChanged.AddListener(i =>
        {
            SettingsManager.Instance.SetIsolationMode(true);
            SettingsManager.Instance.SetIsolatedSpeaker(ids[i]);
        });

        // 4) Font size slider
        fontSizeSlider.minValue = 10;
        fontSizeSlider.maxValue = 100;
        fontSizeSlider.value = S.fontSize;
        fontSizeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetFontSize);

        // 5) Color dropdown
        var colorNames = new List<string> { "White", "Yellow", "Cyan", "Green" };
        colorDropdown.ClearOptions();
        colorDropdown.AddOptions(colorNames);
        var colors = new Color[] { Color.white, Color.yellow, Color.cyan, Color.green };
        int ci = Array.FindIndex(colors, c => c.Equals(S.captionColor));
        colorDropdown.value = (ci >= 0 ? ci : 0);
        colorDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetCaptionColor);

        // 6) Position dropdown
        var posNames = new List<string> {
            "TopLeft", "TopRight", "BottomLeft", "BottomRight", "Center"
        };
        positionDropdown.ClearOptions();
        positionDropdown.AddOptions(posNames);
        positionDropdown.value = (int)S.textPosition;
        positionDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetTextPosition);

        // 7) Wire up NoiseAnalyzer with a fully-qualified call
        var noiseAnalyzer = UnityEngine.Object.FindAnyObjectByType<NoiseAnalyzer>();
        if (noiseAnalyzer != null)
        {
            noiseAnalyzer.warningIcon = warningIcon;
            noiseAnalyzer.spectrogramView = spectrogramView;
        }
    }
}
