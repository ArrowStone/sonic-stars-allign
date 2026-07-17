using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraDependentPostprocessing : MonoBehaviour
{
    public AudioLowPassFilter filter;
    public LayerMask waterLayer;
    public Volume volume;
    public ScriptableRendererFeature rendererFeature;
    public AudioSource WindAudioSource;
    public AudioSource WaterAudioSource;

    void Awake()
    {
        SetWater(false);
    }

    void OnTriggerStay(Collider other)
    {
        if (FrameworkUtility.CompareLayer(other.gameObject.layer, waterLayer))
            SetWater(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (FrameworkUtility.CompareLayer(other.gameObject.layer, waterLayer))
            SetWater(false);
    }

    void SetWater(bool water)
    {
        filter.enabled = water;
        volume.enabled = water;
        rendererFeature.SetActive(water);
        WindAudioSource.mute = water;
        WaterAudioSource.mute = !water;
    }
}
