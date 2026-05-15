using UnityEngine;
using UnityEngine.Events;

public class Automation_ImpactForce : MonoBehaviour, IAutomation
{
    public Vector3 Force;
    public UnityEvent InteractionEvent;
    public bool ReflectVelocityIfOpposite;

    #region Util

    private Collider _refCollider;

    #endregion Util

    private void Start()
    {
        _refCollider = GetComponent<Collider>();
    }

    public PosRot Execute(Sonic_PlayerStateMachine _ctx)
    {
        InteractionEvent.Invoke();
        Vector3 fr = transform.rotation * Force;
        if (Vector3.Dot(_ctx.GroundNormal, fr) > 0.25)
        {
            _ctx.MachineTransition(PlayerStates.Air);
        }

        if(ReflectVelocityIfOpposite && Vector3.Dot(_ctx.Velocity, fr) <= 0)
        {
            _ctx.Velocity = Vector3.Reflect(_ctx.Velocity, fr.normalized);
        }

        _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.Velocity, fr.normalized) + fr;
        Vector3 prevDirection = _ctx.PlayerDirection;
        _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized;

        // Shouldn't really happen ut it does so
        if(_ctx.PlayerDirection.Equals(Vector3.zero))
        {
            _ctx.PlayerDirection = prevDirection;
        }   

        PosRot _transfrm = new()
        {
            Position = _refCollider.ClosestPoint(_refCollider.bounds.center + fr.normalized),
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }
}