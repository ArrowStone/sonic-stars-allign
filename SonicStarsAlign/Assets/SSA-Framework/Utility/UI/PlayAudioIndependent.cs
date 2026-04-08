using UnityEngine;
using UnityEngine.SceneManagement;

// Used for one-time Audio Sources which can stay active after switching scenes, used mainly in UI.
public class AudioPlayIndependent : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] bool destroyAfterUI;

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
        else if (destroyAfterUI && SceneManager.GetActiveScene().buildIndex > 4) // For title music
        {
            Destroy(gameObject);
        }
    }
}
