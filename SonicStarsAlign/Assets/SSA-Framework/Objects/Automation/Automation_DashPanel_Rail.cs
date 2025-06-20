using UnityEngine;
using UnityEngine.Events;

public class Automation_DashPanel_Rail : MonoBehaviour, IAutomation
{
    public Automation_GrindRail SplineRef;
    public float Force;

    [Space]
    public bool Set;

    public UnityEvent InteractionEvent;

    public PosRot Execute(Sonic_PlayerStateMachine _ctx)
    {
        float _vel = Set || _ctx.SplnHandler.SpeedMultiplier < Force ? Force : _ctx.SplnHandler.SpeedMultiplier;
        SplineRef.Execute(_ctx, _ctx.Rb.position);
        _ctx.SplnHandler.SpeedMultiplier = _vel;

        PosRot _transfrm = new()
        {
            Position = transform.position,
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }
}