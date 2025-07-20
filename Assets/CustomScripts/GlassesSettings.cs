using UnityEngine;

// Put it in the global namespace so everything can see it:
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
    public int fontSize = 48;
    public float captionScale = 1f;

    [Header("Voice Isolation")]
    [Tooltip("RMS threshold above which the warning icon appears")]
    public float noiseThreshold = 0.5f;
    public bool isolateSpeakers = true;

    [Header("Naming")]
    public Color nameTextColor = Color.yellow;

    [Header("Speaker Isolation")]
    public bool isolateMode = false;
    public int isolatedSpeaker = 0;

    [Header("Text Placement")]
    public TextPosition textPosition = TextPosition.Center;

    [Header("HUD Positioning")]
    [Tooltip("Offset from the camera for your world-space HUD (in meters)")]
    public Vector3 hudOffset = new Vector3(0f, 0f, 2f);
}
