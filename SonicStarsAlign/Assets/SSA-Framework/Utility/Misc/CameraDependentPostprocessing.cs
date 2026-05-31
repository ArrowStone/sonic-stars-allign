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

    void Awake()
    {
        filter.enabled = false;
        volume.enabled = false;
        rendererFeature.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {
        if(FrameworkUtility.CompareLayer(other.gameObject.layer, waterLayer))
        {
            filter.enabled = true;
            volume.enabled = true;
            rendererFeature.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(FrameworkUtility.CompareLayer(other.gameObject.layer, waterLayer))
        {
            filter.enabled = false;
            volume.enabled = false;
            rendererFeature.SetActive(false);
        }
    }
}
