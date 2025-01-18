using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class Automation_LinearAutomation : MonoBehaviour
{
    public SplineContainer spline;
    public SplineType splineType;

    [Space]
    [Header("Forces")]
    [SerializeField] private AnimationCurve SpeedCurve;

    [SerializeField] private float speedMultiplier;
    [SerializeField] private Vector3 _offset;

    public UnityEvent InteractionEvent;
    
    public void Execute(Sonic_PlayerStateMachine _ctx)
    {
        if (_ctx.SplnHandler.ActiveSpline != spline)
        {
            _ctx.SplnHandler.SplineSetup(spline, splineType, SpeedCurve, speedMultiplier, _offset, 0, false);
            _ctx.SplnHandler.SetTangent(_ctx.transform.up);
            _ctx.MachineTransition(PlayerStates.LinearAutomation);
            InteractionEvent.Invoke();
        }
    }
}