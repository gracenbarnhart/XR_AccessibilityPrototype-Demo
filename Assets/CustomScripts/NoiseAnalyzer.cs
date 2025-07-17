using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Listens to the mic audio clip, computes RMS each frame,
/// and fires OnNoiseAboveThreshold whenever RMS exceeds settings.noiseThreshold.
/// Also drives a UI Image (alarmIcon) to turn red when noise is “too loud.”
/// </summary>
public class NoiseDetector : MonoBehaviour
{
    [Tooltip("Reference your SpeechToTextManager's mic clip")]
    public AudioClip micClip;
    [Tooltip("Number of samples per analysis frame (power-of-two, e.g. 1024)")]
    public int fftSize = 1024;
    [Tooltip("UI image to flash when noise is too loud")]
    public Image alarmIcon;

    public static event Action OnNoiseAboveThreshold;
    private float[] samples;
    private int micFreq;
    private string micDevice;

    void Start()
    {
        // grab your micClip from the STT manager
        micDevice = Microphone.devices[0];
        micFreq = SpeechToTextManager.Instance.micClip.frequency;
        micClip = SpeechToTextManager.Instance.micClip;

        samples = new float[fftSize];
    }

    void Update()
    {
        if (micClip == null) return;

        // 1) RMS volume
        micClip.GetData(samples, Microphone.GetPosition(micDevice) - fftSize);
        float sum = 0;
        for (int i = 0; i < fftSize; i++)
            sum += samples[i] * samples[i];
        float rms = Mathf.Sqrt(sum / fftSize);

        // 2) compare to threshold
        float thresh = SettingsManager.Instance.settings.noiseThreshold;
        bool noisy = rms > thresh;

        // 3) flash alarm icon
        if (alarmIcon != null)
            alarmIcon.color = noisy ? Color.red : Color.white;

        // 4) fire event
        if (noisy)
            OnNoiseAboveThreshold?.Invoke();
    }
}
