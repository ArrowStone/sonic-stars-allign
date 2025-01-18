using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine.Events;

public class Automation_PoleSwing : MonoBehaviour, IAutomation
{
    public float2 Range;
    public float TimeSpeed;
    public SplineContainer spline;
    public SplineType splineType;

    [Space]
    [Header("Forces")]
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private float speedMultiplier;
    [SerializeField] private Vector3 offset;
    public UnityEvent InteractionEvent;

    public PosRot Execute(Sonic_PlayerStateMachine _ctx)
    {
        _ctx.MachineTransition(PlayerStates.Pole);
        _ctx.SplnHandler._ctxPole = this;

        _ctx.Velocity = Vector3.zero;
        _ctx.PlayerDirection = transform.forward;

        PosRot _transfrm = new()
        {
            Position = transform.position,
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }

    public void PoleLaunch(Sonic_PlayerStateMachine _ctx, float _time)
    {
        int _state = 1;
        if(_time >= Range.x && _time <= Range.y)
        {
            _state = 0;
        }

        if (_ctx.SplnHandler.ActiveSpline != spline)
        {
            _ctx.SplnHandler.SplineSetup(spline, splineType, speedCurve, speedMultiplier, offset, 0, false, false, _state);
            _ctx.SplnHandler.SetTangent(_ctx.transform.up);
            _ctx.MachineTransition(PlayerStates.LinearAutomation);
            InteractionEvent.Invoke();
        }
    }
}