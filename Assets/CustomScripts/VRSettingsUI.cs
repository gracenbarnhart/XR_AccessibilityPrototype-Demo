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
    private bool isCollapsed = false;

    void OnEnable()
    {
        PinUI();
        ApplyCollapseState();
    }

    void Start()
    {
        if (collapseButton != null)
            collapseButton.onClick.AddListener(() =>
            {
                isCollapsed = !isCollapsed;
                ApplyCollapseState();
                CoroutineRunner.Run(RepinWhenActiveNextFrame());
            });

        if (isolateToggle != null)
        {
            isolateToggle.isOn = S.isolateMode;
            isolateToggle.onValueChanged.AddListener(SettingsManager.Instance.SetIsolationMode);
        }

        if (speakerDropdown != null)
        {
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
        }

        if (fontSizeSlider != null)
        {
            fontSizeSlider.minValue = 10;
            fontSizeSlider.maxValue = 100;
            fontSizeSlider.value = S.fontSize;
            fontSizeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetFontSize);
        }

        if (colorDropdown != null)
        {
            var colorNames = new List<string> { "White", "Yellow", "Cyan", "Green" };
            colorDropdown.ClearOptions();
            colorDropdown.AddOptions(colorNames);
            var colors = new Color[] { Color.white, Color.yellow, Color.cyan, Color.green };
            int ci = Array.FindIndex(colors, c => c.Equals(S.captionColor));
            colorDropdown.value = (ci >= 0 ? ci : 0);
            colorDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetCaptionColor);
        }

        if (positionDropdown != null)
        {
            var posNames = new List<string> { "TopLeft", "TopRight", "BottomLeft", "BottomRight", "Center" };
            positionDropdown.ClearOptions();
            positionDropdown.AddOptions(posNames);
            positionDropdown.value = (int)S.textPosition;
            positionDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetTextPosition);
        }

        var noiseAnalyzer = UnityEngine.Object.FindAnyObjectByType<NoiseAnalyzer>();
        if (noiseAnalyzer != null) noiseAnalyzer.warningIcon = warningIcon;

        PinUI();
        ApplyCollapseState();
    }

    void LateUpdate()
    {
        ForceAttachAndPosition(namePanel, new Vector2(-231f, 22f), new Vector2(300f, 82.7f), Vector3.one);
        ForceAttachAndPosition(isolateToggle != null ? isolateToggle.gameObject : null, new Vector2(121f, 86f), new Vector2(160f, 20f), new Vector3(2f, 2f, 1f));
    }

    void ForceAttachAndPosition(GameObject go, Vector2 anchoredPos, Vector2 size, Vector3 scale)
    {
        if (go == null) return;
        var rt = go.transform as RectTransform;
        if (rt == null) return;
        if (rt.parent != transform) rt.SetParent(transform, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localRotation = Quaternion.identity;
        rt.localScale = scale;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        var ownCanvas = go.GetComponent<Canvas>();
        if (ownCanvas) ownCanvas.overrideSorting = false;
    }

    void ApplyCollapseState()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (collapseButton != null && t == collapseButton.transform) continue;
            t.gameObject.SetActive(!isCollapsed);
        }
    }

    IEnumerator RepinWhenActiveNextFrame()
    {
        yield return null;
        while (!gameObject.activeInHierarchy) yield return null;
        PinUI();
        ApplyCollapseState();
    }

    void PinUI()
    {
        ForceAttachAndPosition(namePanel, new Vector2(-231f, 22f), new Vector2(300f, 82.7f), Vector3.one);
        ForceAttachAndPosition(isolateToggle != null ? isolateToggle.gameObject : null, new Vector2(121f, 86f), new Vector2(160f, 20f), new Vector3(2f, 2f, 1f));
    }
}
