using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoScreenManager : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    public RawImage VideoDisplay;
    public RenderTexture RenderTexture;
    public GameObject VideoCanvas;
    private void Start()
    {
        VideoCanvas.SetActive(false);
        VideoPlayer.loopPointReached += OnVideoEnd;
    }
    public void PlayVideo()
    {
        VideoCanvas.SetActive(true);
        VideoPlayer.targetTexture = RenderTexture;
        VideoDisplay.texture = RenderTexture;
        VideoPlayer.Play();
    }
    private void OnVideoEnd(VideoPlayer vp)
    {
        VideoCanvas.SetActive(false);
        VideoPlayer.Stop();
    }
}