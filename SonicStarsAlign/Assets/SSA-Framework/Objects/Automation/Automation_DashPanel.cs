using UnityEngine;
using UnityEngine.Events;

public class Automation_DashPanel : MonoBehaviour, IAutomation
{
    public Vector3 Normal;
    public Vector3 Force;
    public Vector3 Offset;

    [Space]
    public bool Set;

    public UnityEvent InteractionEvent;

    public PosRot Execute(Sonic_PlayerStateMachine _ctx)
    {
        Vector3 _norm = Normal.magnitude < 0.1f ? transform.up.normalized : Normal.normalized;
        Vector3 _fr = transform.rotation * Force;
        Vector3 _vel = Set || Vector3.Dot(_ctx.Velocity, _fr.normalized) < _fr.magnitude ? _fr : Vector3.Project(_ctx.Velocity, _fr.normalized);
        Vector3 _pos = transform.position + (transform.rotation * Offset);
        

        if (_ctx.GroundCast.Execute(_pos, -_norm))
        {
            _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
            _ctx.MachineTransition(PlayerStates.Ground);
        }
        else
        {
            _ctx.MachineTransition(PlayerStates.Air);
        }

        _ctx.Velocity = _vel;
        _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized;

        PosRot _transfrm = new()
        {
            Position = _ctx.Rb.position,
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }
}