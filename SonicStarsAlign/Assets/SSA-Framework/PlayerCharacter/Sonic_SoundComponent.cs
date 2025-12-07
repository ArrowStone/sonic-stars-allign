using UnityEngine;

// Handles the player characters' sounds. Mainly exists because I want to keep all the sound clip references in one place.
public class Sonic_SoundComponent : MonoBehaviour
{
    [SerializeField] public AudioSource audioSource;

    // Automation sounds are stored in the automation objects because I cant be bothered.
    [Header ("Moves")]
    public AudioClip jumpSound;
    public AudioClip homingSound;
    public AudioClip rollSound;
    public AudioClip bounceSound;
    public AudioClip lightDashSound;

    [Header ("Movement")]
    public AudioClip railGrindSound;
    public AudioClip railSwitchSound;
    public AudioClip railLandSound;

    [Header ("Collectables")]
    public AudioClip ringSound;
    public AudioClip goalRingSound;

    public void PlaySound(AudioClip sound)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(sound);
    }
}
