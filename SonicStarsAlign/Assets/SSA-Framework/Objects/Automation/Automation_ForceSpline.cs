using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Events;

public class Automation_ForceSpline : MonoBehaviour
{
    public SplineContainer spline;
    public SplineType splineType;

    [Space]
    [Header("Forces")]
    [SerializeField] private AnimationCurve SpeedCurve;

    [SerializeField] private float speedMultiplier;
    [SerializeField] private Vector3 _offset;

    public UnityEvent InteractionEvent;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            Execute(_ctx);
        }
    }

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