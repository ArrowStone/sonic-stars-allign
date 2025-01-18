using UnityEngine;
using UnityEngine.Events;

public class Automation_ImpactForce : MonoBehaviour, IAutomation
{
    public Vector3 Force;
    public UnityEvent InteractionEvent;

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

        _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.Velocity, fr.normalized) + fr;
        _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized;

        PosRot _transfrm = new()
        {
            Position = _refCollider.ClosestPoint(_refCollider.bounds.center + fr.normalized),
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }
}