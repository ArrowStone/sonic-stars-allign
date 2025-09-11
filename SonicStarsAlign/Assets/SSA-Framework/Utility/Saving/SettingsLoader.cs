using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Code for loading and applying settings in the levels. Every parameter has to be hardcoded.
public class SettingsLoader : MonoBehaviour
{
    public VolumeProfile profile;
    public UniversalRenderPipelineAsset pipelineAsset;
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
    void Awake()
    {
        Camera cam = Camera.main;
        //Bloom bloom;
        UniversalAdditionalCameraData cameraData = cam.GetUniversalAdditionalCameraData(); // Provides access to antialiasing

        float viewDist = PlayerPrefs.GetFloat("viewDist");
        int AAMode = PlayerPrefs.GetInt("aaMode");
        int AAQuality = PlayerPrefs.GetInt("aaQuality");
        float renderScale = PlayerPrefs.GetFloat("resScale");

        //profile.TryGet<Bloom>(out bloom);
        //profile.TryGet<MotionBlur>(out motionBlur);

        cam.farClipPlane = 100f + (viewDist * 900f);
        cameraData.antialiasing = AAModes[AAMode];
        cameraData.antialiasingQuality = SMAAQualities[AAQuality];
        cameraData.taaSettings.quality = TAAQualities[AAQuality];
        pipelineAsset.renderScale = renderScale;
    }
}
