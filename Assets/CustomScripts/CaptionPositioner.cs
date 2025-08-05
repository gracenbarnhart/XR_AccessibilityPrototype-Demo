using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CaptionPositioner : MonoBehaviour
{
    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogError($"No SettingsManager found in scene – CaptionPositioner on '{name}' will do nothing.");
            return;
        }

        SettingsManager.Instance.OnTextPositionChanged += UpdatePosition;

        
        UpdatePosition(SettingsManager.Instance.settings.textPosition);
    }

    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnTextPositionChanged -= UpdatePosition;
    }

    void UpdatePosition(TextPosition pos)
    {
        Vector2 anchor, pivot;

        switch (pos)
        {
            case TextPosition.TopLeft:
                anchor = pivot = new Vector2(0, 1);
                break;
            case TextPosition.TopRight:
                anchor = pivot = new Vector2(1, 1);
                break;
            case TextPosition.BottomLeft:
                anchor = pivot = new Vector2(0, 0);
                break;
            case TextPosition.BottomRight:
                anchor = pivot = new Vector2(1, 0);
                break;
            case TextPosition.CenterLeft:
                anchor = pivot = new Vector2(0, 0.5f);
                break;
            case TextPosition.CenterRight:
                anchor = pivot = new Vector2(1, 0.5f);
                break;
            case TextPosition.Center:
            default:
                anchor = pivot = new Vector2(0.5f, 0.5f);
                break;
        }

        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
    }
}
