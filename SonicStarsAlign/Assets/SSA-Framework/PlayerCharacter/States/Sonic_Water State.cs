using UnityEngine;

public class Sonic_WaterState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _inWater;
    private bool _onSurface;

    public Sonic_WaterState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _inWater = true;
        _onSurface = false;

        // Scale gravity for floaty effect in water
        _ctx.Gravity = _ctx.Chp.Gravity * _ctx.Chp.WaterGravityScale;

        // Check if player is moving fast enough to "run" on top of water
        if (_ctx.HorizontalVelocity.magnitude >= _ctx.Chp.WaterRunThreshold)
        {
            _onSurface = true;
            _ctx.Physics_Snap(_ctx.Rb.position + Vector3.up * 0.1f);
        }
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;

        WaterMovement(_delta);
        WaterRotation();
        WaterSwitchConditions();

        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        // Restore normal gravity
        _ctx.Gravity = _ctx.Chp.Gravity;
    }

    #region Util

    private void WaterMovement(float _delta)
    {
        // Target horizontal velocity based on input
        Vector3 targetVel = _ctx.InputVector * _ctx.Chp.WaterSpeedCap;

        // Apply acceleration/deceleration
        _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, targetVel, _ctx.Chp.WaterDeceleration * _delta);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.WaterSpeedCap);

        // Vertical movement: floating effect or jump
        if (!_onSurface)
        {
            _ctx.VerticalVelocity += _ctx.Gravity * _delta;
            _ctx.VerticalVelocity = Vector3.ClampMagnitude(_ctx.VerticalVelocity, _ctx.Chp.WaterMaxFallSpeed);
        }
        else
        {
            _ctx.VerticalVelocity = Vector3.zero; // stay on surface
        }

        // Combine horizontal and vertical
        _ctx.Velocity = _ctx.HorizontalVelocity + _ctx.VerticalVelocity;

        _ctx.Physics_Snap(_ctx.Rb.position + _ctx.Velocity * _delta);
    }

    private void WaterRotation()
    {
        if (_ctx.InputVector.magnitude > 0.1f)
        {
            _ctx.PlayerDirection = _ctx.InputVector;
            _ctx.Physics_Rotate(_ctx.PlayerDirection, Vector3.up);
        }
    }

    private void WaterSwitchConditions()
    {
        // Jumping or leaving water transitions to Air
        if (!_inWater || _ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.VerticalVelocity = Vector3.up * _ctx.Chp.WaterJumpStrength;
            _ctx.MachineTransition(PlayerStates.Air);
        }
    }

    #endregion Util
}