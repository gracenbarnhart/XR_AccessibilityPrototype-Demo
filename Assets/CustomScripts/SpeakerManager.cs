using UnityEngine;
using System.Collections.Generic;

public class SpeakerManager : MonoBehaviour
{
    public static SpeakerManager Instance { get; private set; }
    Dictionary<int, string> names = new Dictionary<int, string>();

    void Awake() => Instance = this;

    public void SetName(int speakerId, string name) => names[speakerId] = name;

    public string GetName(int speakerId) =>
        names.ContainsKey(speakerId) ? names[speakerId] : $"Speaker {speakerId}";
}

