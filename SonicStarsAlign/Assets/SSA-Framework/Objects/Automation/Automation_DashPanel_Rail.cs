using UnityEngine;
using UnityEngine.Events;

public class Automation_DashPanel_Rail : MonoBehaviour
{
    public Automation_GrindRail SplineRef;
    public float Force;

    [Space]
    public bool Set;

    public UnityEvent InteractionEvent;

    public PosRot Execute(Sonic_PlayerStateMachine _ctx)
    {
       float _vel = Set || _ctx.SplnHandler.SpeedMultiplier < Force ? Force : _ctx.SplnHandler.SpeedMultiplier;

        if (_ctx.CurrentEstate is not PlayerStates.RailGrinding)
        {
            SplineRef.Execute(_ctx, _ctx.Rb.position);
        }

        _ctx.SplnHandler.SpeedMultiplier = _vel;
        _ctx.TriggerBuffer = SplineRef.GetComponent<Collider>();

        PosRot _transfrm = new()
        {
            Position = _ctx.Rb.position,
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }
}
