using UnityEngine;

public class Automation_Fan : MonoBehaviour
{
    public AnimationCurve Force;
    public Sonic_PlayerStateMachine ctx;

    public void Update()
    {
        if (ctx == null)
        {
            return;
        }

        float _fr = Force.Evaluate(Vector3.Dot(transform.position - ctx.transform.position, transform.up)) * Time.deltaTime;
        Vector3 _force = transform.up * _fr;

        if (Vector3.Dot(ctx.GroundNormal, _force) > 0.25)
        {
            ctx.MachineTransition(PlayerStates.Air);
        }

        ctx.Velocity += _force;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            ctx = _ctx;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        ctx = null;
    }
}