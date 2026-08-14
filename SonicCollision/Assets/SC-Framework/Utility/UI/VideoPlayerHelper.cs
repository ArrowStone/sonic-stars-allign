using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoPlayerHelper : MonoBehaviour
{
    [SerializeField] VideoPlayer video;
    [SerializeField] int sceneToExitTo = 2;

    void Awake()
    {
        InputComponent inputComponent = GetComponent<InputComponent>();
        IEnumerator WaitTillEnd()
        {
            yield return new WaitUntil(() => video.isPlaying);
            yield return new WaitWhile(() => video.isPlaying && !inputComponent.StartInput.WasPressedThisFrame());
            SceneManager.LoadScene(sceneToExitTo);
        }
        StartCoroutine(WaitTillEnd());
    }
}
