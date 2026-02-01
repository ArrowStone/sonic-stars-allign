using UnityEngine;

public class Sonic_SweepKickState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private float _kickTimer;
    private bool _hasHit;

    // Tunables
    private readonly float _kickDuration = 0.4f;       // total length of animation
    private readonly float _kickSpeedBoost = 6f;       // small push forward if successful
    private readonly float _kickDamageRadius = 1.5f;   // hitbox size
    private readonly float _kickDamageWindow = 0.15f;  // when the kick can actually hit

    public Sonic_SweepKickState(Sonic_PlayerStateMachine machine)
    {
        _ctx = machine;
    }

    public void EnterState()
    {
        Debug.Log("Sweep Kick");
        _kickTimer = 0f;
        _hasHit = false;

		_ctx.Anim.SetTrigger("Kick");

        // lock input for the move
        _ctx.HorizontalVelocity = _ctx.PlayerDirection * Mathf.Max(_ctx.HorizontalVelocity.magnitude, _ctx.Chp.BaseSpeed);
        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
    }

    public void UpdateState()
    {
        float delta = Time.deltaTime;
        _kickTimer += delta;

        // check hit window
        if (_kickTimer >= _kickDamageWindow && !_hasHit)
        {
            TryDoKickDamage();
        }

        // end the move
        if (_kickTimer >= _kickDuration)
        {
			
            _ctx.MachineTransition(PlayerStates.Ground);
        }
    }

    public void FixedUpdateState()
    {
        // keep momentum going while kicking
        _ctx.Physics_ApplyVelocity();
    }

    public void LateUpdateState() { }

    public void ExitState()
    {
        // reset or cleanup if needed
    }

    private void TryDoKickDamage()
    {
        _hasHit = true;

        // Overlap sphere forward from Sonic�s position
        Vector3 kickOrigin = _ctx.Rb.transform.position + _ctx.PlayerDirection * 1f;
        Collider[] hits = Physics.OverlapSphere(kickOrigin, _kickDamageRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // TODO: replace with your enemy damage pipeline
                Debug.Log("Sweep Kick hit: " + hit.name);

                // give a little forward boost
                _ctx.Velocity += _ctx.PlayerDirection * _kickSpeedBoost;
                break;
            }
        }
    }
}