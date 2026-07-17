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
        _kickTimer = 0f;
        _hasHit = false;

        _ctx.Anim.SetTrigger("Kick");
        _ctx.Snd.PlaySound("Slide");

        // Lock input for the move
        _ctx.HorizontalVelocity = _ctx.PlayerDirection * Mathf.Max(_ctx.HorizontalVelocity.magnitude, _ctx.Chp.BaseSpeed);
        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
    }

    public void UpdateState() { }

    public void FixedUpdateState()
    {
        float _delta = Time.deltaTime;

        _ctx.HomingCheck();

        _kickTimer += _delta;

        // Check hit window
        if (_kickTimer >= _kickDamageWindow && !_hasHit)
        {
            TryDoKickDamage();
        }


        // Jumping out of the kick
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.Jump();
            return;
        }

        // Leaving the ground
        if (!GroundCheck())
        {
            _ctx.MachineTransition(PlayerStates.Air);
            return;
        }

        // End the move
        if (_kickTimer >= _kickDuration)
        {
            _ctx.MachineTransition(PlayerStates.Ground);
            return;
        }

        GroundApplication(_delta);
    }

    public void LateUpdateState() { }

    public void ExitState()
    {
    }

    private void TryDoKickDamage()
    {
        _hasHit = true;

        // Overlap sphere forward from Sonic's position
        Vector3 kickOrigin = _ctx.Rb.transform.position + _ctx.PlayerDirection * 1f;
        Collider[] hits = Physics.OverlapSphere(kickOrigin, _kickDamageRadius, _ctx.homingTargetLayer);

        foreach (var hit in hits)
        {
            // TODO: replace with your enemy damage pipeline
            Debug.Log("Sweep Kick hit: " + hit.name);

            // Give a little forward boost
            _ctx.Velocity += _ctx.PlayerDirection * _kickSpeedBoost;
            break;
        }
    }

    private bool GroundCheck()
    {
        return _ctx.GroundCast.Execute(_ctx.transform.position, -_ctx.GroundNormal) &&
            Vector3.Angle(_ctx.GroundCast.HitInfo.normal, _ctx.GroundNormal) <= _ctx.Chp.MaxGroundDeviation;
    }

    private void GroundApplication(float _delta)
    {
        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized * _ctx.Velocity.magnitude;
        _ctx.PlayerDirection = _ctx.HorizontalVelocity.normalized;
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover);
        _ctx.Physics_ApplyVelocity();

        _ctx.Physics_Rotate(_ctx.PlayerDirection, Vector3.Lerp(_ctx.transform.up, _ctx.GroundCast.HitInfo.normal, _delta * _ctx.Chp.RotationSmoothingSpeed));
    }
}