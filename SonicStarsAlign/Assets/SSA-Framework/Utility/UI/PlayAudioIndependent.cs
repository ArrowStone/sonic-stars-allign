using UnityEngine;
using UnityEngine.SceneManagement;

// Used for one-time Audio Sources which can stay active after switching scenes, used mainly in UI.
public class AudioPlayIndependent : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] int destroyAfterBuildIndex = 10000;

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
        else if (SceneManager.GetActiveScene().buildIndex > destroyAfterBuildIndex) // For title music
        {
            Destroy(gameObject);
        }
    }
}
