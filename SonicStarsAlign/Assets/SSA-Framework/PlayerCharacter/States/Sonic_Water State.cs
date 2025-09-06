using UnityEngine;

public class Sonic_WaterState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;

    public Sonic_WaterState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.GroundNormal = -_ctx.Gravity;
        _ctx.VerticalVelocity = Vector3.zero;
    }

    public void UpdateState()
    {
        float delta = Time.deltaTime;

        // Apply floaty water physics
        WaterMovement(delta);
        WaterGravity(delta);

        // Exit conditions
        WaterSwitchConditions();

        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState() { }
    public void LateUpdateState() { }
    public void ExitState() { }

    #region Water Physics

    private void WaterMovement(float delta)
    {
        // Basic deceleration
        _ctx.HorizontalVelocity = Vector3.MoveTowards(
            _ctx.HorizontalVelocity,
            Vector3.zero,
            _ctx.Chp.WaterDeceleration * delta
        );

        // Clamp speed underwater
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(
            _ctx.HorizontalVelocity,
            _ctx.Chp.WaterSpeedCap
        );
    }

    private void WaterGravity(float delta)
    {
        // Apply reduced gravity while submerged
        _ctx.VerticalVelocity += _ctx.Gravity * _ctx.Chp.WaterGravityScale * delta;

        // Clamp fall speed
        if (_ctx.VerticalVelocity.magnitude > _ctx.Chp.WaterMaxFallSpeed)
        {
            _ctx.VerticalVelocity = Vector3.ClampMagnitude(
                _ctx.VerticalVelocity,
                _ctx.Chp.WaterMaxFallSpeed
            );
        }
    }

    #endregion

    #region Exit Conditions

    private void WaterSwitchConditions()
    {
        // Exit if no longer submerged
        if (!_ctx.IsInWater())
        {
            if (_ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal))
            {
                _ctx.MachineTransition(PlayerStates.Ground);
            }
            else
            {
                _ctx.MachineTransition(PlayerStates.Air);
            }
            return;
        }

        // Exit if moving fast enough to "run on water"
        if (_ctx.HorizontalVelocity.magnitude >= _ctx.Chp.WaterRunThreshold)
        {
            if (_ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal))
            {
                _ctx.MachineTransition(PlayerStates.Ground);
            }
            else
            {
                _ctx.MachineTransition(PlayerStates.Air);
            }
        }

        // Jump inside water
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.VerticalVelocity = Vector3.up * _ctx.Chp.WaterJumpStrength;
        }
    }

    #endregion
}