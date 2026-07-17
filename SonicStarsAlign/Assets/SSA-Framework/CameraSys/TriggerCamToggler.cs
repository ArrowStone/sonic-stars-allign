using Unity.Cinemachine;
using UnityEngine;

public class TriggerCamToggler : MonoBehaviour
{
    [SerializeField] CinemachineCamera CMCam;
    [SerializeField] bool CamEnable = true;
    [SerializeField] bool CamDisable = true;
    void OnTriggerEnter(Collider other)
    {
        if (CamEnable && other.CompareTag("Player"))
            CMCam.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (CamDisable && other.CompareTag("Player"))
            CMCam.enabled = false;
    }
}
