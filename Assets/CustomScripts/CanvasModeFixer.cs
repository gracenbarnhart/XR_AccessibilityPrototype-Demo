using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasModeAutoFixer : MonoBehaviour
{
    public RectTransform[] uiElements;
    public RectTransform forceParent;

    Vector3[] savedLocalPos;
    Vector2[] savedAnchoredPos;

    Canvas canvas;
    RenderMode lastMode;
    bool restoring;

    void OnEnable()
    {
        canvas = GetComponentInParent<Canvas>();
        if (!canvas) canvas = GetComponent<Canvas>();
        lastMode = canvas ? canvas.renderMode : RenderMode.ScreenSpaceOverlay;
        Allocate();
        SaveNow();
    }

    void LateUpdate()
    {
        if (!canvas) return;
        SaveNow();
        if (canvas.renderMode != lastMode && !restoring)
        {
            lastMode = canvas.renderMode;
            StartCoroutine(RestoreNextFrame());
        }
    }

    void Allocate()
    {
        if (uiElements == null) uiElements = new RectTransform[0];
        savedLocalPos = new Vector3[uiElements.Length];
        savedAnchoredPos = new Vector2[uiElements.Length];
    }

    void SaveNow()
    {
        if (savedLocalPos == null || savedLocalPos.Length != uiElements.Length) Allocate();
        for (int i = 0; i < uiElements.Length; i++)
        {
            var rt = uiElements[i];
            if (!rt) continue;
            savedLocalPos[i] = rt.localPosition;
            savedAnchoredPos[i] = rt.anchoredPosition;
        }
    }

    IEnumerator RestoreNextFrame()
    {
        restoring = true;
        yield return null;
        for (int i = 0; i < uiElements.Length; i++)
        {
            var rt = uiElements[i];
            if (!rt) continue;

            if (forceParent && rt.parent != forceParent)
                rt.SetParent(forceParent, false);

            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            rt.anchoredPosition = savedAnchoredPos[i];
            var lp = savedLocalPos[i];
            lp.z = 0f;
            rt.localPosition = lp;

            var ownCanvas = rt.GetComponent<Canvas>();
            if (ownCanvas) ownCanvas.overrideSorting = false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        restoring = false;
    }
}
