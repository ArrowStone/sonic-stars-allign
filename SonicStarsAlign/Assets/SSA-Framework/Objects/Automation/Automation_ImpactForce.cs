using UnityEngine;
using UnityEngine.Events;

public class Automation_ImpactForce : MonoBehaviour
{

    public Vector3 ForceNormal;

    [SerializeField]
    private AutomationForce force;

    public UnityEvent InteractionEvent;
    public UnityEvent ForceEvent;
    public UnityEvent NoForceEvent;

    #region Util
    private Sonic_PlayerStateMachine _ctx;
    private Collider _triggerCl;
    private bool _done = false;
    private Vector3 _norm;
    private Vector3 _vel;
    #endregion Util

    private void Awake()
    {
        _triggerCl = GetComponent<Collider>();
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (_ctx != null)
            return;

        InteractionEvent.Invoke();
        _done = false;

        if (collision.transform.TryGetComponent(out _ctx))
        {
            LaunchChecks();
        }
    }

    private void LaunchChecks()
    {
        _norm = ForceNormal.magnitude < 0.1f ? transform.up.normalized : ForceNormal.normalized;
        _vel = Vector3.ProjectOnPlane(_ctx.Rb.linearVelocity, _norm);

        _ctx.ChangeKinematic(true);
        _ctx.Rb.position = _triggerCl.ClosestPoint(_ctx.Rb.position);
        Execute(force);

        ForceEvent.Invoke();
        _done = true;

    }

    private void Execute(AutomationForce currentForce)
    {
        if (Vector3.Dot(transform.rotation * currentForce.Force, _ctx.GroundCast.HitInfo.normal) > 0)
        {
            _ctx.MachineTransition(PlayerStates.Air);
        }

        _ctx.ChangeKinematic(false);
        if (currentForce.Set)
        {
            _ctx.Rb.linearVelocity = (transform.rotation * currentForce.Force).normalized * currentForce.Force.magnitude;
        }
        else
        {
            _ctx.Rb.linearVelocity = _vel + (transform.rotation * currentForce.Force).normalized * currentForce.Force.magnitude;
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        _ctx = null;
        if (!_done)
        {
            NoForceEvent.Invoke();
        }
    }
}