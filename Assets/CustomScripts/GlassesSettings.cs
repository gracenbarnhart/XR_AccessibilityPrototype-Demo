using UnityEngine;

[CreateAssetMenu(
  menuName = "XR/GlassesSettings",
  fileName = "GlassesSettings"
)]
public class GlassesSettings : ScriptableObject
{
    [Header("Caption")]
    public Color captionColor = Color.white;
    public float captionScale = 1f;

    [Header("Voice Isolation")]
    public float noiseThreshold = 0.5f;
    public bool isolateSpeakers = true;

    [Header("Naming")]
    public Color nameTextColor = Color.yellow;

    // … add whatever other settings you want here …
}
