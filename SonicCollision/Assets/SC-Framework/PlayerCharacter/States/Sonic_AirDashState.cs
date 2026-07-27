using System;
using UnityEngine;

public class Sonic_AirDashState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _airTime;

    public Sonic_AirDashState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc
        _groundDetected = false;
        _airTime = _ctx.Chp.DashLength;
        _ctx.Skid = false;
        #endregion

        #region Velocity
        if (_ctx.InputVector.magnitude > 0) _ctx.PlayerDirection = _ctx.InputVector;
        _ctx.HorizontalVelocity = _ctx.PlayerDirection * _ctx.Chp.DashSpeed;
        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
        #endregion

        #region Animations
        _ctx.ModelManager.ExitBall();
        _ctx.Anim.SetInteger("State", 4);
        #endregion
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.HomingCheck();
        GroundCheck();
        InputRotations();

        _airTime -= _delta;

        AirDashMovement(_delta);
        AirDashSwitchConditions();
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        //_ctx.Physics_ApplyVelocity();
    }

    #region Util

    private bool GroundCheck()
    {
        _groundDetected = _ctx.GroundCast.Execute(_ctx.Rb.transform.position, -_ctx.Gravity);
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity) <= _ctx.Chp.MaxGroundDeviation;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(-_ctx.Gravity, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, -_ctx.Gravity) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, -_ctx.Gravity);

        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);
    }

    private void AirDashMovement(float _delta)
    {
        float _turnStrength = _ctx.ChrTurn.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
        if (_ctx.InputVector.magnitude > 0) _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.InputVector, _turnStrength, 0);
        float speed = Math.Max(_ctx.Velocity.magnitude, _ctx.Chp.DashSpeed);
        _ctx.HorizontalVelocity = _ctx.PlayerDirection * speed;
        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
    }

    private void AirDashSwitchConditions()
    {
        if (_ctx.HomingTargetDetector.TargetDetected && _ctx.Input.AttackInput.WasPressedThisFrame())
        {
            _ctx.Snd.PlaySound("Homing");
            _ctx.MachineTransition(PlayerStates.HomingAttack);
        }
        if (_ctx.RingDetector.TargetDetected && _ctx.Input.LightDashInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.LightSpeedDash);
        }
        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.Snd.PlaySound("Bounce");
            _ctx.MachineTransition(PlayerStates.Bounce);
        }
        if (_groundDetected)
        {
            _ctx.MachineTransition(PlayerStates.Ground);
        }
        if (_airTime <= 0)
        {
            _ctx.MachineTransition(PlayerStates.Air);
        }
    }
    #endregion Util
}