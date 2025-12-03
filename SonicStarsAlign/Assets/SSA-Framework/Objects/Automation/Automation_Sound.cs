using UnityEngine;

public class Automation_Sound : MonoBehaviour
{
    [SerializeField] private AudioClip sound;
    [SerializeField] private AudioSource source;
    public void PlaySound()
    {
        source.PlayOneShot(sound);
    }
}
