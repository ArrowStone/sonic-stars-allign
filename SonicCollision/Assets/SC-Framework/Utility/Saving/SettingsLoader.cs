using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;
using System;
using UnityEngine.Audio;

// Code for loading and applying settings in the levels. Every parameter has to be hardcoded.
public class SettingsLoader : MonoBehaviour
{
    [Header("References")]
    public CamPoint_NormalPlayer pointPlayer;
    public CinemachineCamera CMCamera;
    public VolumeProfile profile;
    public UniversalRenderPipelineAsset pipelineAsset;
    public AudioMixer Mixer;
    public UniversalRenderPipelineAsset URPAsset;

    [Header("Qualities")]
    public int[,] Resolutions =
    {
        {0,0},
        {3840,2160},
        {2560,1440},
        {1920,1080},
        {1280,720},
        {720,480},
    };
    public int[] Framerates =
    {
        30,
        60,
        90,
        120,
        144,
        160,
        240,
        360
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
    public float SoundVolumeMultiplier = 10f;
    public UpscalingFilterSelection[] Filters =
    {
        UpscalingFilterSelection.Auto,
        UpscalingFilterSelection.Linear,
        UpscalingFilterSelection.Point,
        UpscalingFilterSelection.FSR // Tf is STP
    };
    public int[] ShadowResolutions =
    {
        256,
        512,
        1024,
        2048,
        4096,
        8192
    };
    public MotionBlurQuality[] MBlurQualities =
    {
        MotionBlurQuality.Low,
        MotionBlurQuality.Medium,
        MotionBlurQuality.High
    };
    public AudioSpeakerMode[] AudioModes =
    {
        AudioSpeakerMode.Mono,
        AudioSpeakerMode.Stereo,
        AudioSpeakerMode.Quad,
        AudioSpeakerMode.Surround,
        AudioSpeakerMode.Mode5point1,
        AudioSpeakerMode.Mode7point1,
        AudioSpeakerMode.Prologic
    };

    void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        // Manually setting all the values from the settings

        Camera cam = Camera.main;
        UniversalAdditionalCameraData cameraData = cam.GetUniversalAdditionalCameraData(); // Provides access to antialiasing

        FullScreenMode[] str = { FullScreenMode.Windowed, FullScreenMode.MaximizedWindow, FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen };

        int displayMode = PlayerPrefs.GetInt("displayMode");
        int res = PlayerPrefs.GetInt("resolution");
        int bufferFrames = PlayerPrefs.GetInt("vSync");
        bool vsync = bufferFrames > 0;
        int targetFramerate = PlayerPrefs.GetInt("targetFramerate");
        bool runInBackground = PlayerPrefs.GetInt("runInBackground") > 0;
        float viewDist = PlayerPrefs.GetFloat("renderDistance");
        int modelDetail = PlayerPrefs.GetInt("modelDetail");
        int textureResolution = PlayerPrefs.GetInt("textureResolution");
        int AAMode = PlayerPrefs.GetInt("aaMode");
        int AAQuality = PlayerPrefs.GetInt("aaQuality");
        float renderScale = PlayerPrefs.GetFloat("renderScale");
        int scalingFilter = PlayerPrefs.GetInt("scalingFilter");
        int shadowResolution = PlayerPrefs.GetInt("shadowResolution");
        float shadowDistance = PlayerPrefs.GetFloat("shadowDistance") * 50f;

        int mBlurQuality = PlayerPrefs.GetInt("mBlurQuality");
        float mBlurIntensity = PlayerPrefs.GetFloat("mBlurIntensity");

        float mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
        float joySensitivity = PlayerPrefs.GetFloat("joySensitivity");

        bool homingOnJump = PlayerPrefs.GetInt("homingMapping") > 0; // True = jump button, False = attack button

        float masterVolume = PlayerPrefs.GetFloat("masterVolume") - 0.5f;
        float musicVolume = PlayerPrefs.GetFloat("musicVolume") - 0.5f;
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume") - 0.5f;
        int audioOutputMode = PlayerPrefs.GetInt("soundOutputMode");

        if (res > 0)
        {
            Screen.SetResolution(Resolutions[res, 0], Resolutions[res, 1], displayMode == 3);
        }
        Screen.fullScreenMode = str[displayMode];
        QualitySettings.vSyncCount = vsync ? 0 : 1;
        QualitySettings.maxQueuedFrames = bufferFrames;
        QualitySettings.maximumLODLevel = 2 - modelDetail;
        QualitySettings.globalTextureMipmapLimit = 2 - textureResolution;

        Application.targetFrameRate = Framerates[targetFramerate];

        cam.farClipPlane = 100f + (viewDist * 900f);
        if (CMCamera) // Absent in menu scenes
        { CMCamera.Lens.FarClipPlane = 100f + (viewDist * 900f); }
        cameraData.antialiasing = AAModes[AAMode];
        cameraData.antialiasingQuality = SMAAQualities[AAQuality];
        cameraData.taaSettings.quality = TAAQualities[AAQuality];
        pipelineAsset.renderScale = Math.Max(0.1f, renderScale);
        URPAsset.upscalingFilter = Filters[scalingFilter];
        URPAsset.mainLightShadowmapResolution = ShadowResolutions[shadowResolution];
        URPAsset.shadowDistance = shadowDistance;

        foreach (VolumeComponent c in profile.components)
        {
            switch (c.name)
            {
                case "MotionBlur":
                    {
                        MotionBlur mBlur = (MotionBlur)c;
                        mBlur.intensity.value = mBlurIntensity;
                        mBlur.quality.value = MBlurQualities[mBlurQuality];
                        break;
                    }
            }
        }

        Sonic_PlayerStateMachine _ctx = FindAnyObjectByType<Sonic_PlayerStateMachine>();
        if (_ctx) _ctx.HomingOnJump = homingOnJump;

        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        audioConfig.speakerMode = AudioModes[audioOutputMode];
        AudioSettings.Reset(audioConfig);

        Mixer.SetFloat("masterVolume", masterVolume * SoundVolumeMultiplier);
        Mixer.SetFloat("musicVolume", musicVolume * SoundVolumeMultiplier);
        Mixer.SetFloat("sfxVolume", sfxVolume * SoundVolumeMultiplier);
        Mixer.SetFloat("voiceVolume", sfxVolume * SoundVolumeMultiplier);
    }
}
