using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip sound;
    [SerializeField] private AudioSource source;
    public void PlaySound()
    {
        source.PlayOneShot(sound);
    }
}
