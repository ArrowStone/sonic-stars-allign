using UnityEngine;

public class Sonic_WallRunState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private float _timer;

    public Sonic_WallRunState(Sonic_PlayerStateMachine machine)
    {
        _ctx = machine;
    }

    public void EnterState()
    {
        _timer = 0f;

        // Lock vertical velocity
        _ctx.VerticalVelocity = Vector3.zero;

        // Determine slide direction along the wall
        Vector3 slideDir = Vector3.Cross(_ctx.WallRunNormal, -_ctx.Gravity.normalized).normalized;
        _ctx.HorizontalVelocity = slideDir * _ctx.Chp.WallRunSpeed;

        _ctx.ChangeKinematic(false);
        _ctx.OnWall = true;
    }

    public void UpdateState()
    {
        float dt = Time.deltaTime;
        _timer += dt;

        // Max time safety
        if (_timer > _ctx.Chp.MaxWallRunTime)
        {
            LeaveWall();
            return;
        }

        // Player jumps → wall jump
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            DoWallJump();
            return;
        }

        // Check if we're still touching a valid wall
        if (!StillOnWall())
        {
            LeaveWall();
            return;
        }

        ApplyPhysics(dt);
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState() { }
    public void LateUpdateState() { }

    public void ExitState()
    {
        _ctx.OnWall = false;
    }

    private bool StillOnWall()
    {
        RaycastHit hit;
        Vector3 origin = _ctx.Rb.position;

        // Check still facing the original wall
        if (Physics.Raycast(origin, -_ctx.WallRunNormal, out hit, _ctx.Chp.WallAttachCheckDistance))
        {
            float verticalDot = Mathf.Abs(hit.normal.y);

            // still wall-like
            return verticalDot < _ctx.Chp.MinWallDot;
        }

        return false;
    }

    private void ApplyPhysics(float dt)
    {
        // Reduced gravity
        _ctx.VerticalVelocity += _ctx.Gravity * _ctx.Chp.WallRunGravityScale * dt;

        // Maintain wall slide direction
        Vector3 slideDir = Vector3.Cross(_ctx.WallRunNormal, -_ctx.Gravity.normalized).normalized;
        _ctx.HorizontalVelocity = slideDir * _ctx.Chp.WallRunSpeed;
    }

    private void DoWallJump()
    {
        Vector3 jumpDir = (_ctx.WallRunNormal + -_ctx.Gravity.normalized).normalized;

        _ctx.VerticalVelocity = jumpDir * _ctx.Chp.WallJumpStrength;
        _ctx.HorizontalVelocity = jumpDir * _ctx.Chp.WallJumpStrength;

        _ctx.MachineTransition(PlayerStates.Air);
    }

    private void LeaveWall()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }
}
