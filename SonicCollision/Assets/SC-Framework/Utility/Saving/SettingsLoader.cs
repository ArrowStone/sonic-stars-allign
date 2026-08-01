using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

// Code for loading and applying settings in the levels. Every parameter has to be hardcoded.
public class SettingsLoader : MonoBehaviour
{
    [Header("References")]
    public CamPoint_NormalPlayer pointPlayer;
    public CinemachineCamera CMCamera;
    public VolumeProfile profile;
    public UniversalRenderPipelineAsset pipelineAsset;

    [Header("Qualities")]
    public int[,] resolutions =
    {
        {0,0},
        {3840,2160},
        {2560,1440},
        {1920,1080},
        {1280,720},
        {720,480},
    };
    public AntialiasingMode[] AAModes = // Need to correspond to the order in the settings menu
    {
        AntialiasingMode.None,
        AntialiasingMode.FastApproximateAntialiasing,
        AntialiasingMode.SubpixelMorphologicalAntiAliasing,
        AntialiasingMode.TemporalAntiAliasing
    };
    public AntialiasingQuality[] SMAAQualities = // Same here
    {
        AntialiasingQuality.Low,
        AntialiasingQuality.Medium,
        AntialiasingQuality.High
    };
    public TemporalAAQuality[] TAAQualities = // Can choose only three out of five for user friendliness reasons and because I'm too lazy to code the switch between 3 and 5 options
    {
        TemporalAAQuality.VeryLow,
        TemporalAAQuality.Medium,
        TemporalAAQuality.VeryHigh
    };

    void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        Camera cam = Camera.main;
        UniversalAdditionalCameraData cameraData = cam.GetUniversalAdditionalCameraData(); // Provides access to antialiasing

        FullScreenMode[] str = { FullScreenMode.Windowed, FullScreenMode.MaximizedWindow, FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen };

        FullScreenMode displayMode = str[PlayerPrefs.GetInt("displayMode")];
        int res = PlayerPrefs.GetInt("resolution");
        int bufferFrames = PlayerPrefs.GetInt("vSync");
        bool vsync = bufferFrames > 0;
        bool runInBackground = PlayerPrefs.GetInt("runInBackground") > 0;
        //float viewDist = PlayerPrefs.GetFloat("viewDist");
        float viewDist = 0.1f;
        int AAMode = PlayerPrefs.GetInt("aaMode");
        int AAQuality = PlayerPrefs.GetInt("aaQuality");
        float renderScale = PlayerPrefs.GetFloat("resScale");

        float mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
        float joySensitivity = PlayerPrefs.GetFloat("joySensitivity");

        if (res > 0)
        {
            Screen.SetResolution(resolutions[res, 0], resolutions[res, 1], displayMode == FullScreenMode.ExclusiveFullScreen);
        }
        Screen.fullScreenMode = displayMode;
        QualitySettings.vSyncCount = vsync ? 0 : 1;
        QualitySettings.maxQueuedFrames = bufferFrames;

        if (renderScale == 0)
        {
            renderScale = 1f;
        }

        cam.farClipPlane = 100f + (viewDist * 900f);
        if (CMCamera) // Absent in menu scenes
        { CMCamera.Lens.FarClipPlane = 100f + (viewDist * 900f); }
        cameraData.antialiasing = AAModes[AAMode];
        cameraData.antialiasingQuality = SMAAQualities[AAQuality];
        cameraData.taaSettings.quality = TAAQualities[AAQuality];
        pipelineAsset.renderScale = renderScale;
    }
}
