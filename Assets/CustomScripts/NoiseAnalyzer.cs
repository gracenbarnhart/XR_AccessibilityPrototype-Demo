using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class NoiseAnalyzer : MonoBehaviour
{
    [Header("Audio Input")]
    [Tooltip("It will grab this AudioSource or autoadd one on this GameObject")]
    public AudioSource micSource;

    [Header("Loud-Noise Alarm")]
    [Tooltip("RMS threshold above which the exclamation icon appears")]
    public float alarmThreshold = 0.5f;
    [Tooltip("Exclamation Image here")]
    public Image warningIcon;

    void Start()
    {
        // micSource fallback
        if (micSource == null)
            micSource = GetComponent<AudioSource>();

        if (micSource == null)
        {
            Debug.LogError("[NoiseAnalyzer] No AudioSource found or assigned!");
            enabled = false;
            return;
        }

        // hide warningIcon at start if assigned
        if (warningIcon != null)
            warningIcon.gameObject.SetActive(false);

        // start the microphone
        micSource.clip = Microphone.Start(null, true, 1, 16000);
        micSource.loop = true;
        while (Microphone.GetPosition(null) <= 0) { }
        micSource.Play();
    }

    void Update()
    {
        // real spectrum data
        const int SPECTRUM_SIZE = 256;
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
    }
}
