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

        // Check if we're still touching a valid wall AND moving at a valid angle
        if (!StillOnWall() || !HasValidWallRunAngle())
        {
            LeaveWall();
            return;
        }


        if (_ctx.Input.JumpInput.IsPressed())
        {
            _ctx.MachineTransition(PlayerStates.WallJump);
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

        if (Physics.Raycast(origin, -_ctx.WallRunNormal, out hit, _ctx.Chp.WallAttachCheckDistance))
        {
            // Dot against gravity
            float gravityDot = Mathf.Abs(Vector3.Dot(hit.normal.normalized, -_ctx.Gravity.normalized));

            // 0 = perfectly vertical wall
            // 1 = floor or ceiling
            return gravityDot <= _ctx.Chp.MaxWallGravityDot;
        }

        return false;
    }

    //Helps fix perpendicular bug
    private bool HasValidWallRunAngle()
    {
        Vector3 velocity = _ctx.Velocity;

        if (velocity.magnitude < _ctx.Chp.WallRunMinSpeed)
            return false;

        Vector3 velDir = velocity.normalized;
        float headOnDot = Mathf.Abs(Vector3.Dot(velDir, _ctx.WallRunNormal.normalized));

        // Reject near-perpendicular impacts
        return headOnDot <= _ctx.Chp.MaxWallHeadOnDot;
    }
    private Vector3 GetWallUpDirection()
    {
        // Up direction constrained to wall surface
        return Vector3.ProjectOnPlane(-_ctx.Gravity.normalized, _ctx.WallRunNormal).normalized;
    }
    private void ApplyPhysics(float dt)
    {
        // Base slide direction
        Vector3 wallForward = Vector3.Cross(_ctx.WallRunNormal, -_ctx.Gravity.normalized).normalized;

        // Wall-aligned up/down direction
        Vector3 wallUp = GetWallUpDirection();

        // Player vertical input (forward/back stick ignored)
        float verticalInput = _ctx.Input.VectorMoveInput.z;

        // Add controlled steering
        Vector3 steerVelocity =
            wallForward * _ctx.Chp.WallRunSpeed +
            wallUp * (verticalInput * _ctx.Chp.WallRunVerticalControl);

        _ctx.HorizontalVelocity = steerVelocity;

        // Light gravity so player slowly slides down if idle
        _ctx.VerticalVelocity += _ctx.Gravity * _ctx.Chp.WallRunGravityScale * dt;
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
