using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public AudioClip clip;

    [Range(0.0f, 1.0f)]
    public float volume;

    [Range(.1f, 2.0f)]
    public float picth;

    [HideInInspector]
    public AudioSource source;

    public string name;

    public bool loop;
}
