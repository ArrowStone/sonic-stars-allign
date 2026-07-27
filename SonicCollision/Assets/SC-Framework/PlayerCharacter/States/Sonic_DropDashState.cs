using UnityEngine;

public class Sonic_DropDashState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _chargeTime;

    public Sonic_DropDashState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc
        _groundDetected = false;
        _chargeTime = 0f;
        _ctx.Skid = false;
        #endregion

        #region Collision
        // Snap slightly downward to prepare for dash
        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
        #endregion

        #region Velocity
        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.HorizontalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
        #endregion
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;

        // Charge input while airborne
        if (!_groundDetected && _ctx.Input.CrouchInput.IsPressed())
        {
            _chargeTime += _delta;
        }

        // Check for ground contact to execute drop dash
        if (GroundCheck())
        {
            LaunchDropDash();
        }

        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.HomingCheck();
        if (_groundDetected)
        {
            // Ground movement after landing from drop dash
            _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal);
            _ctx.Physics_ApplyVelocity();
            GroundSwitchConditions();
        }
        else
        {
            // Midair fall
            _ctx.VerticalVelocity += _ctx.Gravity * _delta;
            _ctx.Physics_ApplyVelocity();
        }
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
    }

    #region Util

    private bool GroundCheck()
    {
        _groundDetected = _ctx.GroundCast.Execute(_ctx.Rb.transform.position, -_ctx.Gravity);
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity) <= _ctx.Chp.MaxGroundDeviation;
    }

    private void LaunchDropDash()
    {
        _groundDetected = true;

        // Launch with velocity based on charge time
        float dashPower = _ctx.Chp.DropDashBaseSpeed + (_ctx.Chp.DropDashChargeMultiplier * Mathf.Clamp(_chargeTime, 0, _ctx.Chp.DropDashMaxCharge));
        _ctx.HorizontalVelocity = _ctx.PlayerDirection * dashPower;
        _ctx.VerticalVelocity = Vector3.zero;

        _ctx.MachineTransition(PlayerStates.Roll); // Enter roll state after drop dash
    }

    private void GroundSwitchConditions()
    {
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.Jump();
            return;
        }

        if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.MinRollSpeed)
        {
            _ctx.MachineTransition(PlayerStates.Ground);
        }
    }
    #endregion Util
}