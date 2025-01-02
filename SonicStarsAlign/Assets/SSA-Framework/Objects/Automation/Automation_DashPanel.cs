using UnityEngine;
using UnityEngine.Events;

public class Automation_DashPanel : MonoBehaviour
{
    [SerializeField] Vector3 normal;
    [SerializeField] private Vector3 offset;
    [SerializeField] private AutomationForce force;

    public UnityEvent InteractionEvent;

    #region Util
    private Sonic_PlayerStateMachine _ctx;
    private Vector3 _norm;
    private Vector3 _pos;
    private Vector3 _vel;
    #endregion Util

    public void OnTriggerEnter(Collider _cl)
    {
        if (_cl == _ctx.TriggerCl)
        {
            return;
        }

        if (_cl.transform.TryGetComponent(out _ctx))
        {
            InteractionEvent.Invoke();
            Initiate();
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        _ctx = null;
    }

    private void Initiate()
    {
        _norm = normal.magnitude < 0.1f ? transform.up.normalized : normal.normalized;
        _vel = Vector3.ProjectOnPlane(_ctx.Rb.linearVelocity, _norm);
        _pos = transform.position + (transform.rotation * offset);

        _ctx.ChangeKinematic(true);
        _ctx.Rb.position = _pos;
        Execute(force);
    }

    private void Execute(AutomationForce currentForce)
    {
        _vel = currentForce.Set || Vector3.Dot(_vel, (transform.rotation * currentForce.Force).normalized) < currentForce.Force.magnitude
            ? transform.rotation * currentForce.Force
            : Vector3.Project(_vel, (transform.rotation * currentForce.Force).normalized);

        if (_ctx.GroundCast.Execute(_pos, -_norm))
        {
            _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
            _ctx.MachineTransition(PlayerStates.Ground);
        }
        else
        {
            _ctx.MachineTransition(PlayerStates.Air);
        }

        _ctx.ChangeKinematic(false);
        _ctx.Velocity = _vel;
    }
}