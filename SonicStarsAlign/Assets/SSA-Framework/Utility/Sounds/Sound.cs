using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class SoundType
{
    public string Name;
    public AudioClip[] Clip;
    public AudioMixerGroup Channel;
}
