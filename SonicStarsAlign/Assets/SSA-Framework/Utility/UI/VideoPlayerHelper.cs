using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoPlayerHelper : MonoBehaviour
{
    [SerializeField] VideoPlayer video;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IEnumerator WaitTillEnd()
        {
            yield return new WaitUntil(() => video.isPlaying);
            yield return new WaitWhile(() => video.isPlaying);
            SceneManager.LoadScene(0);
        }
        StartCoroutine(WaitTillEnd());
    }
}
