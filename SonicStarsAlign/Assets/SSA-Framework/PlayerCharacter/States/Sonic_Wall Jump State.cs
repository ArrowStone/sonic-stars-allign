using UnityEngine;

public class Sonic_WallJumpState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;

    private float _chargeTime;
    private bool _charging;

    public Sonic_WallJumpState(Sonic_PlayerStateMachine machine)
    {
        _ctx = machine;
    }

    public void EnterState()
    {
        _chargeTime = 0f;
        _charging = true;

        // Stop vertical movement so jump charge begins from stable point
        _ctx.VerticalVelocity = Vector3.zero;

        // Optional: reduce horizontal speed to prevent sliding during charge
        _ctx.HorizontalVelocity = Vector3.zero;

        // Make sure physics mode is correct for normal movement
        _ctx.ChangeKinematic(false);
    }

    public void UpdateState()
    {
        float dt = Time.deltaTime;

        // If jump is still held, continue charging
        if (_charging && _ctx.Input.JumpInput.IsPressed())
        {
            _chargeTime += dt;
            _chargeTime = Mathf.Clamp(_chargeTime, 0f, _ctx.Chp.WallJumpMaxCharge);
        }
        else if (_charging && !_ctx.Input.JumpInput.IsPressed())
        {
            // Player released the jump button
            _charging = false;
            PerformJump();
            return;
        }

        // If we somehow lose the wall before jumping, fall
        if (!StillTouchingWall())
        {
            _ctx.MachineTransition(PlayerStates.Air);
            return;
        }

        // No movement until jump; player stays attached
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState() { }

    public void LateUpdateState() { }

    public void ExitState()
    {
        // nothing needed — AirState will take over velocity
    }

    private void PerformJump()
    {
        // Convert charge time into a jump strength
        float t = _chargeTime / _ctx.Chp.WallJumpMaxCharge;

        float strength = Mathf.Lerp(
            _ctx.Chp.WallJumpMinStrength,
            _ctx.Chp.WallJumpMaxStrength,
            t
        );

        // Jump direction = away from the wall + up
        Vector3 jumpDir = (-_ctx.WallRunNormal + Vector3.up).normalized;

        _ctx.VerticalVelocity = jumpDir * strength;
        _ctx.HorizontalVelocity = jumpDir * strength;

        _ctx.MachineTransition(PlayerStates.Air);
    }

    private bool StillTouchingWall()
    {
        RaycastHit hit;
        Vector3 origin = _ctx.Rb.position;

        if (Physics.Raycast(origin, -_ctx.WallRunNormal, out hit, _ctx.Chp.WallAttachCheckDistance))
        {
            float verticalDot = Mathf.Abs(hit.normal.y);
            return verticalDot < _ctx.Chp.MinWallDot;
        }

        return false;
    }
}
