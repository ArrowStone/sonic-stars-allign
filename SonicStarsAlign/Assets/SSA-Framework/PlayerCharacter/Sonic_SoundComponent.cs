using UnityEngine;
using UnityEngine.Audio;

/*Changed this bit to make it easier to add new sounds in the inspector
 
 To add a sound just make an entry in Sounds or CommonSounds, give it a name, 
 sound clip (can be multiple for variation) and an audiomixer.
 to call it simply do _ctx.Snd.PlaySound("YourSoundName");

 */ 

public class Sonic_SoundComponent : MonoBehaviour
{
    private Sonic_PlayerStateMachine _ctx;

    public AudioSource[] AudioSources;
	public AudioSource VoiceAudioSource;
	public AudioSource RailSpinSource;

	public Sounds_Database CommonSounds;
	public SoundType[] Sounds;
	SoundType[] SoundsCombined;

	AudioMixerGroup Mixer;
	AudioSource LatestAudioSource;
	bool playedSound;


    public AudioClip railGrindSound;
    public AudioClip spinUpSound;


    public void Start()
    {
        _ctx = GetComponent<Sonic_PlayerStateMachine>();

		// You can separate sounds from a specific prefab (like character voicelines) and common sounds (dash, jump, etc), then combine them.
		SoundsCombined = new SoundType[Sounds.Length + CommonSounds.Sounds.Length];
		Sounds.CopyTo(SoundsCombined, 0);
        CommonSounds.Sounds.CopyTo(SoundsCombined, Sounds.Length);
    }

	public void PlaySound(string ClipName)
	{
		foreach (SoundType s in SoundsCombined){
			if(s.Name == ClipName){
				if(s.Clip.Length > 1){
					int rand = Random.Range(0, s.Clip.Length);
					Mixer = s.Channel;
					PlayClip(s.Clip[rand]);
				}else{
					Mixer = s.Channel;
					PlayClip(s.Clip[0]);
				}
			}
		}
	}
    public void PlaySound(string ClipName, int SoundInt)
    {
        foreach (SoundType s in SoundsCombined){
            if(s.Name == ClipName){
                Mixer = s.Channel;
                PlayClip(s.Clip[SoundInt]);
            }
        }
    }
    public void PlaySoundOneSource(string ClipName, int SourceInt)
    {
        foreach (SoundType s in SoundsCombined){
            if(s.Name == ClipName){
                if(s.Clip.Length > 1){
                    int rand = Random.Range(0, s.Clip.Length);
                    Mixer = s.Channel;
                    AudioSources[SourceInt].clip = s.Clip[rand];
                    AudioSources[SourceInt].outputAudioMixerGroup = Mixer;
                    AudioSources[SourceInt].Play();
                }else{
                    Mixer = s.Channel;
                    AudioSources[SourceInt].clip = s.Clip[0];
                    AudioSources[SourceInt].outputAudioMixerGroup = Mixer;
                    AudioSources[SourceInt].Play();
                }
            }
        }
    }

    public void InterruptSound(){LatestAudioSource.Stop();}

	void PlayClip(AudioClip Clip)
    {
        int i = 0;
        playedSound = false;

        if(Mixer.name == "Voices"){
            if(!VoiceAudioSource.isPlaying){
                VoiceAudioSource.clip = Clip;
				VoiceAudioSource.Play();
				LatestAudioSource = VoiceAudioSource;
            }
            return;
        }
        while (i < AudioSources.Length) 
        {
            if(!AudioSources[i].isPlaying){
                AudioSources[i].clip = Clip;
                AudioSources[i].outputAudioMixerGroup = Mixer;
                AudioSources[i].Play();
                LatestAudioSource = AudioSources[i];
                playedSound = true;
                break;
            }
            i++;
        }
		//Makes sure to play even if there isnt an audio source available
        if(!playedSound){
            AudioSources[6].clip = Clip;
            AudioSources[6].outputAudioMixerGroup = Mixer;
            AudioSources[6].Play();
            LatestAudioSource = AudioSources[0];
        }
    }

}
