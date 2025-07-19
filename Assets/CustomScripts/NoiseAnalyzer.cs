using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class NoiseAnalyzer : MonoBehaviour
{
    [Header("Audio Input")]
    public AudioSource micSource;

    [Header("Spectrogram")]
    public RawImage spectrogramView;
    const int SPECTRUM_SIZE = 256;
    private Texture2D specTex;
    private float[,] history = new float[64, SPECTRUM_SIZE];

    [Header("Loud‐Noise Alarm")]
    [Tooltip("RMS threshold (0–1) above which the exclamation icon appears")]
    public float alarmThreshold = 0.5f;
    public Image warningIcon;

    void Start()
    {
        // 1) Start and loop the mic on your AudioSource
        micSource.clip = Microphone.Start(null, true, 1, 16000);
        micSource.loop = true;
        while (Microphone.GetPosition(null) <= 0) { }
        micSource.Play();

        // 2) Create the spectrogram texture
        specTex = new Texture2D(SPECTRUM_SIZE, 64, TextureFormat.RGBA32, false);
        specTex.filterMode = FilterMode.Point;
        spectrogramView.texture = specTex;

        // 3) Hide the warning icon until we hit the threshold
        warningIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        // 4) Grab spectrum data
        float[] spectrum = new float[SPECTRUM_SIZE];
        micSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // 5) Compute RMS of the spectrum
        float sum = 0f;
        foreach (var v in spectrum) sum += v * v;
        float rms = Mathf.Sqrt(sum / SPECTRUM_SIZE);

        // 6) Toggle warningIcon only on very loud sounds
        bool isLoud = rms > alarmThreshold;
        if (warningIcon.gameObject.activeSelf != isLoud)
            warningIcon.gameObject.SetActive(isLoud);

        // 7) Shift spectrogram history up one row
        for (int y = 0; y < 63; y++)
            for (int x = 0; x < SPECTRUM_SIZE; x++)
                history[y, x] = history[y + 1, x];

        // 8) Add the newest spectrum as the bottom row
        for (int x = 0; x < SPECTRUM_SIZE; x++)
            history[63, x] = spectrum[x] * 10f;

        // 9) Draw the history into the texture
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < SPECTRUM_SIZE; x++)
            {
                float v = Mathf.Clamp01(history[y, x]);
                specTex.SetPixel(x, y, Color.Lerp(Color.black, Color.green, v));
            }
        }
        specTex.Apply();
    }
}
