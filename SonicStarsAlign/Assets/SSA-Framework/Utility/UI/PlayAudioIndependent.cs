using UnityEngine;
using UnityEngine.SceneManagement;

// Used for one-time Audio Sources which can stay active after switching scenes, used mainly in UI.
public class AudioPlayIndependent : MonoBehaviour
{
    private AudioSource source;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (!source.isPlaying)
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().buildIndex > 3) // For title music
        {
            Destroy(gameObject);
        }
    }
}
