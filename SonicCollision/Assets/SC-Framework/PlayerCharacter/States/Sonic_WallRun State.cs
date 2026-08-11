using UnityEngine;

public class Sonic_WallRunState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private float _timer;
    private RaycastHit _wallHit;

    public Sonic_WallRunState(Sonic_PlayerStateMachine machine)
    {
        _ctx = machine;
    }

    public void EnterState()
    {
        _ctx.AirDashes = 1;

        _timer = 0f;

        // Lock vertical velocity
        _ctx.VerticalVelocity = Vector3.zero;

        // Determine slide direction along the wall
        Vector3 slideDir = Vector3.Cross(_ctx.WallRunNormal, -_ctx.Gravity.normalized).normalized;
        _ctx.HorizontalVelocity = slideDir * _ctx.Chp.WallRunSpeed;

        _ctx.ChangeKinematic(false);
        _ctx.OnWall = true;

        _ctx.Anim.SetInteger("State", _ctx.WallRunDirection ? 10 : 9);
    }

    public void UpdateState() { }

    public void FixedUpdateState()
    {
        float _delta = Time.deltaTime;
        _timer += _delta;

        _ctx.HomingCheck();

        // Max time safety
        if (_timer > _ctx.Chp.MaxWallRunTime)
        {
            DoWallJump();
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
            DoWallJump();
            return;
        }


        ApplyPhysics(_delta);
        _ctx.Physics_ApplyVelocity();
    }

    public void LateUpdateState() { }

    public void ExitState()
    {
        _ctx.OnWall = false;
    }

    private bool StillOnWall()
    {
        Vector3 origin = _ctx.transform.position;

        // Use the layer defined in the PlayerStateMachine
        if (Physics.Raycast(
            origin,
            -_ctx.WallRunNormal,
            out _wallHit,
            _ctx.Chp.WallAttachCheckDistance,
            _ctx.wallRunLayer      // <-- reference the layer from the state machine
        ))
        {
            // Dot against gravity
            float gravityDot = Mathf.Abs(Vector3.Dot(_wallHit.normal.normalized, -_ctx.Gravity.normalized));

            // 0 = perfectly vertical wall, 1 = floor/ceiling
            return gravityDot <= _ctx.Chp.MaxWallGravityDot;
        }

        return false;
    }



    // Helps fix perpendicular bug
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
        // Direction pointing from the wall
        _ctx.WallRunNormal = _wallHit.normal;

        // Base slide direction
        Vector3 wallForward = Vector3.Cross(_ctx.WallRunNormal, -_ctx.Gravity.normalized).normalized;

        // Wall-aligned up/down direction
        Vector3 wallUp = GetWallUpDirection();

        // Player horizontal input (forward/back stick ignored)
        float horizontalInput = -_ctx.Input.VectorMoveInput.x;

        // Applying direction
        if (!_ctx.WallRunDirection)
        {
            wallForward = -wallForward;
            horizontalInput = -horizontalInput;
        }

        // Add controlled steering
        Vector3 steerVelocity =
            wallForward * _ctx.Chp.WallRunSpeed +
            wallUp * (horizontalInput * _ctx.Chp.WallRunVerticalControl);

        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(steerVelocity, _ctx.Chp.WallRunSpeed);

        // Light gravity so player slowly slides down if idle
        _ctx.VerticalVelocity += _ctx.Gravity * _ctx.Chp.WallRunGravityScale * dt;

        // Adjust player rotation
        _ctx.PlayerDirection = wallForward;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.WallRunNormal);

        // Adjust player position
        _ctx.Physics_Snap(_wallHit.point + _wallHit.normal * 0.34f);
    }

    private void DoWallJump()
    {
        Vector3 jumpDir = (_ctx.WallRunNormal + -_ctx.Gravity.normalized).normalized;

        _ctx.VerticalVelocity = jumpDir * _ctx.Chp.WallJumpStrength;
        _ctx.HorizontalVelocity = jumpDir * _ctx.Chp.WallJumpStrength;
        _ctx.Physics_ApplyVelocity();

        _ctx.MachineTransition(PlayerStates.Air);
    }

}
