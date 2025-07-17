using UnityEngine;
using UnityEngine.UI;

public class NoiseAnalyzer : MonoBehaviour
{
    public AudioSource micSource;
    public Image warningIcon;
    public RawImage spectrogramView;
    [Tooltip("0–1 threshold for noise alarm")]
    public float threshold = 0.1f;

    const int SPECTRUM_SIZE = 256;

    private Texture2D specTex;
    private float[,] history;

    void Start()
    {
        // now it's safe to create Texture2D
        specTex = new Texture2D(SPECTRUM_SIZE, 64, TextureFormat.RGBA32, false);
        specTex.filterMode = FilterMode.Point;
        history = new float[64, SPECTRUM_SIZE];
        spectrogramView.texture = specTex;
    }

    void Update()
    {
        float[] spectrum = new float[SPECTRUM_SIZE];
        micSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // Noise alarm: compute RMS
        float rms = 0f;
        foreach (var v in spectrum) rms += v * v;
        rms = Mathf.Sqrt(rms / SPECTRUM_SIZE);
        warningIcon.enabled = (rms > threshold);

        // shift history up one row
        for (int y = 0; y < 63; y++)
            for (int x = 0; x < SPECTRUM_SIZE; x++)
                history[y, x] = history[y + 1, x];

        // write new bottom row
        for (int x = 0; x < SPECTRUM_SIZE; x++)
            history[63, x] = spectrum[x] * 10f;

        // draw into specTex
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < SPECTRUM_SIZE; x++)
                specTex.SetPixel(x, y, Color.Lerp(Color.black, Color.green, history[y, x]));

        specTex.Apply();
    }
}
