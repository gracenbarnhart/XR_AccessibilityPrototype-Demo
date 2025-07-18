using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VRSettingsUI : MonoBehaviour
{
    [Header("Root & Content")]
    public RectTransform panelRoot;      // your SettingsPanel / PanelRoot
    public GameObject panelContent;   // the child GameObject holding all controls

    [Header("Collapse")]
    public Button collapseButton;

    [Header("Isolation")]
    public Toggle isolateToggle;
    public TMP_Dropdown speakerDropdown;

    [Header("Caption Style")]
    public Slider fontSizeSlider;
    public TMP_Dropdown colorDropdown;
    public TMP_Dropdown positionDropdown;

    [Header("Noise Display")]
    public Image noiseIcon;
    public RawImage spectrogramView;

    private GlassesSettings S => SettingsManager.Instance.settings;

    void Start()
    {
        // Collapse/Expand only the content, leave the root (and collapse button) visible
        if (collapseButton != null && panelContent != null)
        {
            collapseButton.onClick.AddListener(() =>
                panelContent.SetActive(!panelContent.activeSelf)
            );
        }

        // Isolation toggle
        isolateToggle.isOn = S.isolateMode;
        isolateToggle.onValueChanged.AddListener(SettingsManager.Instance.SetIsolationMode);

        // Speaker dropdown
        speakerDropdown.ClearOptions();
        var names = new List<string>();
        var mgr = FindObjectOfType<AcousticEventManager>();
        if (mgr != null)
            foreach (var src in mgr.sources)
                names.Add(src.name);
        speakerDropdown.AddOptions(names);
        speakerDropdown.value = S.isolatedSpeaker;
        speakerDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetIsolatedSpeaker);

        // Font size slider
        fontSizeSlider.minValue = 10;
        fontSizeSlider.maxValue = 100;
        fontSizeSlider.value = S.fontSize;
        fontSizeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetFontSize);

        // Color dropdown
        var colorNames = new List<string> { "White", "Yellow", "Cyan", "Green" };
        colorDropdown.ClearOptions();
        colorDropdown.AddOptions(colorNames);
        var colors = new Color[] { Color.white, Color.yellow, Color.cyan, Color.green };
        int ci = Array.FindIndex(colors, c => c.Equals(S.captionColor));
        colorDropdown.value = (ci >= 0 ? ci : 0);
        colorDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetCaptionColor);

        // Position dropdown
        var posNames = new List<string>
        {
            "TopLeft", "TopRight", "BottomLeft", "BottomRight", "Center"
        };
        positionDropdown.ClearOptions();
        positionDropdown.AddOptions(posNames);
        positionDropdown.value = (int)S.textPosition;
        positionDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetTextPosition);

        // Hook up noise & spectrogram
        var noiseAnalyzer = FindObjectOfType<NoiseAnalyzer>();
        if (noiseAnalyzer != null)
        {
            noiseAnalyzer.warningIcon = noiseIcon;
            noiseAnalyzer.spectrogramView = spectrogramView;
        }
    }
}
