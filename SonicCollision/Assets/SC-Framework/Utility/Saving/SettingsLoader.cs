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
        {1440,1080},
        {1920,1080},
        {720,480},
        {480,240}
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

        bool fullScreen = PlayerPrefs.GetInt("fullsc") == 1;
        int res = PlayerPrefs.GetInt("res");
        //float viewDist = PlayerPrefs.GetFloat("viewDist");
        float viewDist = 0.1f;
        int AAMode = PlayerPrefs.GetInt("aaMode");
        int AAQuality = PlayerPrefs.GetInt("aaQuality");
        float renderScale = PlayerPrefs.GetFloat("resScale");

        float mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
        float joySensitivity = PlayerPrefs.GetFloat("joySensitivity");

        if (res > 0)
        {
            Screen.SetResolution(resolutions[res, 0], resolutions[res, 1], fullScreen);
        }
        else
        {
            //Screen.fullScreen = fullScreen;
        }

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
