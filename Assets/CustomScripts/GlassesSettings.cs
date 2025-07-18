using UnityEngine;

public enum TextPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

[CreateAssetMenu(
  menuName = "XR/GlassesSettings",
  fileName = "GlassesSettings"
)]
public class GlassesSettings : ScriptableObject
{
    [Header("Caption")]
    public Color captionColor = Color.white;
    [Range(10, 100)]
    public int fontSize = 48;       // ← already present
    public float captionScale = 1f; // optional

    [Header("Voice Isolation")]
    public float noiseThreshold = 0.5f;
    public bool isolateSpeakers = true;

    [Header("Naming")]
    public Color nameTextColor = Color.yellow;

    [Header("Speaker Isolation")]
    public bool isolateMode = false;
    public int isolatedSpeaker = 0;

    [Header("Text Placement")]      // ← NEW
    public TextPosition textPosition = TextPosition.Center;
}
