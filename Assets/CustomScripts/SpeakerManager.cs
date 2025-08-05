using UnityEngine;
using System.Collections.Generic;

public class SpeakerManager : MonoBehaviour
{
    public static SpeakerManager Instance { get; private set; }
    Dictionary<int, string> names = new Dictionary<int, string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // names playprefs
        for (int id = 0; id < 10; id++)
        {
            var key = $"speakerName_{id}";
            if (PlayerPrefs.HasKey(key))
            {
                names[id] = PlayerPrefs.GetString(key);
            }
        }
    }

    /// <summary>
    /// name in popup
    /// </summary>
    public void SetName(int speakerId, string name)
    {
        names[speakerId] = name;
        PlayerPrefs.SetString($"speakerName_{speakerId}", name);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// the saved name or a default if none.
    /// </summary>
    public string GetName(int speakerId)
    {
        return names.TryGetValue(speakerId, out var n)
            ? n
            : $"Speaker {speakerId}";
    }

    /// <summary>
    /// a copy of all saved speakerId name mappings.
    /// </summary>
    public Dictionary<int, string> GetAllNames()
    {
        return new Dictionary<int, string>(names);
    }
}
