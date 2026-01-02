using UnityEngine;

// Handles the player characters' sounds. Mainly exists because I want to keep all the sound clip references in one place.
public class Sonic_SoundComponent : MonoBehaviour
{
    private Sonic_PlayerStateMachine _ctx;
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

    [Header ("Footsteps")]
    public AudioClip[] concreteFootsteps;

    [Header ("Damage")]
    public AudioClip ringScatterSound;

    public void Start()
    {
        _ctx = GetComponent<Sonic_PlayerStateMachine>();
    }

    public void PlaySound(AudioClip sound)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(sound);
    }
    public void PlayRandom(AudioClip[] sounds)
    {
        //audioSource.Stop();
        audioSource.PlayOneShot(sounds[(byte) Random.Range(0f, sounds.Length-1)]);
    }
    public void PlayFootstep()
    {
        if(_ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal))
        {
            PlayRandom(concreteFootsteps);
        }
    }
}
