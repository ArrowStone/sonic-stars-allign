using UnityEngine;

// Used to restart music when changing audio output mode because it stops otherwise.
public class AudioChangeMusicReset : MonoBehaviour
{
    AudioSource source;

    public void ResetMusic()
    {
        source.Stop();
        source.Play();
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
    }
}
