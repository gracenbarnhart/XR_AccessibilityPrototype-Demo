using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class NoiseAnalyzer : MonoBehaviour
{
    [Header("Audio Input")]
    [Tooltip("It will grab this AudioSource or auto-add one on this GameObject")]
    public AudioSource micSource;

    [Header("Spectrogram")]
    [Tooltip("RawImage where the spectrogram will draw")]
    public RawImage spectrogramView;

    const int SPECTRUM_SIZE = 256;
    private Texture2D specTex;
    private float[,] history;

    [Header("Loud‐Noise Alarm")]
    [Tooltip("RMS threshold (0–1) above which the exclamation icon appears")]
    public float alarmThreshold = 0.5f;
    [Tooltip("Drag your exclamation Image here")]
    public Image warningIcon;

    void Awake()
    {
        // initialize the history buffer
        history = new float[64, SPECTRUM_SIZE];
    }

    void Start()
    {
        // 1) micSource fallback
        if (micSource == null)
        {
            micSource = GetComponent<AudioSource>();
            if (micSource == null)
            {
                Debug.LogError("[NoiseAnalyzer] No AudioSource found or assigned!");
                enabled = false;
                return;
            }
        }

        // 2) spectrogramView must be assigned
        if (spectrogramView == null)
        {
            Debug.LogError("[NoiseAnalyzer] spectrogramView (RawImage) not assigned!");
            enabled = false;
            return;
        }

        // 3) hide warningIcon at start (if assigned)
        if (warningIcon != null)
            warningIcon.gameObject.SetActive(false);

        // 4) start the microphone
        micSource.clip = Microphone.Start(null, true, 1, 16000);
        micSource.loop = true;
        while (Microphone.GetPosition(null) <= 0) { }
        micSource.Play();

        // 5) create the spectrogram texture
        specTex = new Texture2D(SPECTRUM_SIZE, 64, TextureFormat.RGBA32, false);
        specTex.filterMode = FilterMode.Point;
        spectrogramView.texture = specTex;
    }

    void Update()
    {
        // — DRAW A QUICK SINE WAVE TO TEST THE PIPELINE — 
        // Comment out below loop and uncomment the FFT block once you verify visuals.
        float t = Time.time * 5f;
        // shift up
        for (int y = 0; y < 63; y++)
            for (int x = 0; x < SPECTRUM_SIZE; x++)
                history[y, x] = history[y + 1, x];
        // fill bottom row with sine
        for (int x = 0; x < SPECTRUM_SIZE; x++)
            history[63, x] = 0.5f + 0.5f * Mathf.Sin((x / (float)SPECTRUM_SIZE) * Mathf.PI * 10 + t);
        // draw
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < SPECTRUM_SIZE; x++)
                specTex.SetPixel(x, y,
                    Color.Lerp(Color.black, Color.green, Mathf.Clamp01(history[y, x])));
        specTex.Apply();
        // — END DEMO SINE —

        /* Uncomment this block for real audio once you see the sine waterfall:
        // real spectrum
        float[] spectrum = new float[SPECTRUM_SIZE];
        micSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // compute RMS
        float sum = 0f;
        foreach (var v in spectrum) sum += v * v;
        float rms = Mathf.Sqrt(sum / SPECTRUM_SIZE);

        // toggle exclamation on very loud
        if (warningIcon != null)
        {
            bool isLoud = rms > alarmThreshold;
            if (warningIcon.gameObject.activeSelf != isLoud)
                warningIcon.gameObject.SetActive(isLoud);
        }

        // shift up and draw into texture exactly as above...
        */
    }
}
