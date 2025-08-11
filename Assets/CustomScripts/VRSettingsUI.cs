using System;
using System.Collections;
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

    [Header("Speaker Naming (inline)")]
    public GameObject namePanel;
    public TMP_InputField nameInputField;
    public Button saveNameBtn;

    [Header("Noise Display Wiring")]
    public Image warningIcon;

    private GlassesSettings S => SettingsManager.Instance.settings;

    void OnEnable()
    {
        // When the HUD/panel becomes active again, repin immediately.
        PinUI();
    }

    void Start()
    {
        if (collapseButton != null)
            collapseButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var go = transform.GetChild(i).gameObject;
                    if (go != collapseButton.gameObject) go.SetActive(!go.activeSelf);
                }
                // Wait a frame AFTER layout changes, but do it safely
                CoroutineRunner.Run(RepinWhenActiveNextFrame());
            });

        isolateToggle.isOn = S.isolateMode;
        isolateToggle.onValueChanged.AddListener(SettingsManager.Instance.SetIsolationMode);

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
            CoroutineRunner.Run(RepinWhenActiveNextFrame());
        });

        fontSizeSlider.minValue = 10;
        fontSizeSlider.maxValue = 100;
        fontSizeSlider.value = S.fontSize;
        fontSizeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetFontSize);

        var colorNames = new List<string> { "White", "Yellow", "Cyan", "Green" };
        colorDropdown.ClearOptions();
        colorDropdown.AddOptions(colorNames);
        var colors = new Color[] { Color.white, Color.yellow, Color.cyan, Color.green };
        int ci = Array.FindIndex(colors, c => c.Equals(S.captionColor));
        colorDropdown.value = (ci >= 0 ? ci : 0);
        colorDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetCaptionColor);

        var posNames = new List<string> { "TopLeft", "TopRight", "BottomLeft", "BottomRight", "Center" };
        positionDropdown.ClearOptions();
        positionDropdown.AddOptions(posNames);
        positionDropdown.value = (int)S.textPosition;
        positionDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetTextPosition);

        var noiseAnalyzer = UnityEngine.Object.FindAnyObjectByType<NoiseAnalyzer>();
        if (noiseAnalyzer != null) noiseAnalyzer.warningIcon = warningIcon;

        PinUI();
    }

    // Wait a frame, then only repin if our GO is active again.
    IEnumerator RepinWhenActiveNextFrame()
    {
        yield return null; // wait for layout/activation toggles to settle
        // if the panel was hidden, wait until it's active
        while (!gameObject.activeInHierarchy) yield return null;
        PinUI();
    }

    void PinUI()
    {
        if (namePanel != null)
        {
            var nrt = namePanel.GetComponent<RectTransform>();
            if (nrt != null)
            {
                nrt.SetParent(transform, false);
                nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0.5f);
                nrt.pivot = new Vector2(0.5f, 0.5f);
                nrt.localScale = Vector3.one;
                nrt.sizeDelta = new Vector2(300f, 82.7f);
                nrt.anchoredPosition = new Vector2(-231f, 22f);
                var lp = nrt.localPosition;
                lp.z = 0.01f;
                nrt.localPosition = lp;
                var fit = namePanel.GetComponent<ContentSizeFitter>();
                if (fit) { fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; fit.verticalFit = ContentSizeFitter.FitMode.Unconstrained; }
            }
        }

        if (isolateToggle != null)
        {
            var trt = isolateToggle.transform as RectTransform;
            if (trt != null)
            {
                trt.SetParent(transform, false);
                trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.localScale = new Vector3(2f, 2f, 1f);
                trt.sizeDelta = new Vector2(160f, 20f);
                trt.anchoredPosition = new Vector2(121f, 86f);
                var lp = trt.localPosition;
                lp.z = 0f;
                trt.localPosition = lp;
                var fit = isolateToggle.GetComponent<ContentSizeFitter>();
                if (fit) { fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; fit.verticalFit = ContentSizeFitter.FitMode.Unconstrained; }
            }
        }
    }
}
