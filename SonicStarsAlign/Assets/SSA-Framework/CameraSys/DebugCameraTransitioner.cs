using UnityEngine;
using UnityEngine.Events;

public class DebugCameraTransitioner : MonoBehaviour
{
    public CamBrain MainCam;

    [Space]
    public CameraTransition Enter;

    public UnityEvent EnterEvent;

    public CameraTransition Exit;
    public UnityEvent ExitEvent;

    private Panel_Collider _triggercl;

    public void OnEnable()
    {
        _triggercl = GetComponent<Panel_Collider>();
        _triggercl.TriggerEnter += TEnter;
        _triggercl.TriggerExit += TExit;
    }

    public void OnDisable()
    {
        _triggercl.TriggerEnter -= TEnter;
        _triggercl.TriggerExit -= TExit;
    }

    private void TEnter(Collider _other)
    {
        if (Enter.Point == null) return;

        var _camPoint = Enter.Point.GetComponent<ICamPoint>();
        MainCam.WieghtCurve = Enter.WeightCurve;
        MainCam.Point = _camPoint;
        MainCam.MachineTransition(CameraStates.Transitioning);

        EnterEvent.Invoke();
    }

    private void TExit(Collider _other)
    {
        if (Exit.Point == null) return;

        var _camPoint = Exit.Point.GetComponent<ICamPoint>();
        MainCam.WieghtCurve = Exit.WeightCurve;
        MainCam.Point = _camPoint;
        MainCam.MachineTransition(CameraStates.Transitioning);

        ExitEvent.Invoke();
    }
}

[System.Serializable]
public struct CameraTransition
{
    public GameObject Point;
    public AnimationCurve WeightCurve;
}