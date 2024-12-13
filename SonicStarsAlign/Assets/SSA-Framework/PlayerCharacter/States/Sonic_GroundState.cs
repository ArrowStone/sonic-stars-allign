﻿using UnityEngine;

public class Sonic_GroundState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _slipState;

    public Sonic_GroundState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _groundDetected = true;

        #endregion Misc

        #region Collision

        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundCast.HitInfo.normal);
        #endregion Collision

        #region Velocity

        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal);
        _ctx.Physics_ApplyVelocity();

        InputRotations();

        #endregion Velocity
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        if (!GroundCheck())
        {
            AirSwitchConditions();
            return;
        }

        GroundApplication(_delta);
        if (SlipCheck())
        {
            Slipment(_delta);
        }
        else
        {
            Movement(_delta);
        }

        GroundSwitchConditions(_delta);
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
    }

    public void ExitState()
    {
    }

    #region Util

    private bool GroundCheck()
    {
        _groundDetected = _ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal);
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, _ctx.GroundNormal) <= _ctx.Chm.MaxGroundDeviation;
    }

    private bool SlipCheck()
    {
        if (_ctx.HorizontalVelocity.magnitude < _ctx.Chm.MinGroundStickSpeed && Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.ESlipAngle)
        {
            _slipState = _ctx.Chm.SlipTime;
            if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.SlopeAngle)
            {
                _slipState = _ctx.Chm.SlipTime;
                _groundDetected = false;
                AirSwitchConditions();
            }
        }
        if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) <= FrameworkUtility.FloorAngle)
        {
            _slipState = 0;
        }
        return _slipState > 0;
    }

    private void GroundApplication(float _delta)
    {
        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized * _ctx.Velocity.magnitude;
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover, _ctx.SnapForce * _delta);

        InputRotations();
        GroundRotation(_delta);
    }

    private void GroundRotation(float _delta)
    {
        float _turnStrength = _ctx.ChrTurn.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
        if (_ctx.InputVector.magnitude >= 0.1 && !_ctx.Skid)
        {
            _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.InputVector, _turnStrength, 0);
        }
        else if (_ctx.HorizontalVelocity.magnitude >= 0.1 && Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.PlayerDirection) > 0)
        {
            _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.HorizontalVelocity.normalized, _turnStrength, 0);
        }

        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundCast.HitInfo.normal);
    }

    private void Movement(float _delta)
    {
        _ctx.Skid = false;
        if (_ctx.InputVector.magnitude > 0.1)
        {
            if (Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector) < _ctx.Chm.TurnDeviationCap)
            {
                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chm.MinBreakSpeed)
                {
                    _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _ctx.Chm.BreakStrength * _delta);
                    _ctx.Skid = true;
                    return;
                }
                else
                {
                    _ctx.HorizontalVelocity = _ctx.InputVector * Vector3.Dot(_ctx.InputVector, _ctx.HorizontalVelocity);
                }
            }

            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chm.BaseSpeed)
            {
                float _acceleration = _ctx.Chm.AccelerationCurve.Evaluate(_ctx.HorizontalVelocity.magnitude) * _delta;
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chm.BaseSpeed);
            }

            if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chm.TurnDeceleration.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }

            float _turnStrength = _ctx.Chm.TurnStrengthCurve.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.InputVector * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }
        else
        {
            float _deceleration = _ctx.Chm.DecelerationCurve.Evaluate(_ctx.HorizontalVelocity.magnitude) * _delta;
            _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _deceleration);
        }
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chm.HardSpeedCap);
    }

    private void Slipment(float _delta)
    {
        _ctx.Skid = false;

        _slipState -= _delta;
        _ctx.HorizontalVelocity += _ctx.Chm.SlopeFactor * _delta * Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chm.HardSpeedCap);
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);

        //  Quaternion cameraRotation = Quaternion.Euler(0, StateMachine.Camera.eulerAngles.y, 0);
        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void GroundSwitchConditions(float _delta)
    {
        if (_ctx.HorizontalVelocity.magnitude < _ctx.Chm.MinGroundStickSpeed && Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.SlopeAngle)
        {
            AirSwitchConditions();
        }

        if (_ctx.Input.CrouchInput.WasPressedThisFrame() && _ctx.HorizontalVelocity.magnitude > _ctx.Chm.MinRollSpeed)
        {
            _ctx.MachineTransition(PlayerStates.Roll);
        }

        if (_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Spindash);
        }

        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.Jump(_delta);
        }
    }

    private void AirSwitchConditions()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }

    #endregion Util
}