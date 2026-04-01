using System.Collections.Generic;
using UnityEngine;

public class Automation_Fan : MonoBehaviour
{
    public Vector3 TargetDisplacement;
    public float ForceMultiplier;
    private float DisplacementMagnitude;
    private Vector3 DisplacementDirection;
    public Sonic_PlayerStateMachine ctx;
    public List<Rigidbody> rbs;

    public void Awake()
    {
        DisplacementMagnitude = TargetDisplacement.magnitude;
        DisplacementDirection = TargetDisplacement.normalized;
    }

    public void FixedUpdate()
    {
        if(!ctx) return;
        foreach (Rigidbody r in rbs)
        {
            if(r.isKinematic)
            {
                // Probably ledge grabbing
                continue;
            }
            //float _fr = Force.Evaluate(Vector3.Dot(transform.position - ctx.transform.position, transform.up)) * Time.deltaTime;
            float _fr =  DisplacementMagnitude - Vector3.Distance(transform.position, ctx.transform.position);
            Vector3 _force = DisplacementDirection * _fr * ForceMultiplier;

            if (ctx != null && Vector3.Dot(ctx.GroundNormal, _force) > 0.25)
            {
                ctx.MachineTransition(PlayerStates.Air);
            }
            r.linearVelocity += _force;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody _rbs) && !rbs.Contains(_rbs))
        {
            rbs.Add(_rbs);
        }
        if (other.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            ctx = _ctx;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody _rbs))
        {
            rbs.Remove(_rbs);
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, TargetDisplacement);
        Gizmos.DrawRay(transform.position + TargetDisplacement, transform.right * 0.5f - transform.up);
        Gizmos.DrawRay(transform.position + TargetDisplacement, - transform.right * 0.5f - transform.up);
    }
}