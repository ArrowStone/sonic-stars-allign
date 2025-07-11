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
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover);

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
        SlopePhysics(_delta);
        if (SlipCheck())
        {
            Slipment(_delta);
            if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.SlopeAngle)
            {
                _groundDetected = false;
                AirSwitchConditions();
            }
        }
        else
        {
            Movement(_delta);
        }

        GroundSwitchConditions();
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        _ctx.RingCheck();
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
        _groundDetected = _ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal);
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, _ctx.GroundNormal) <= _ctx.Chp.MaxGroundDeviation;
    }

    private bool SlipCheck()
    {
        if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.MinGroundStickSpeed && Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.ESlipAngle)
        {
            _slipState = _ctx.Chp.SlipTime;
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
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover);

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
        else if (_ctx.HorizontalVelocity.magnitude >= 0.1 && Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.PlayerDirection) > _ctx.Chp.TurnDeviationCap)
        {
            _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.HorizontalVelocity.normalized, _turnStrength, 0);
        }
        _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.PlayerDirection, _ctx.GroundCast.HitInfo.normal);
        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundCast.HitInfo.normal);
    }

    private void Movement(float _delta)
    {
        _ctx.Skid = false;
        if (_ctx.InputVector.magnitude > 0.1)
        {
            if (Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector) < _ctx.Chp.TurnDeviationCap)
            {
                float _break = _ctx.Chp.BreakStrength * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _break);
                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinBreakSpeed)
                {
                    _ctx.Skid = true;
                    return;
                }
                else if (_ctx.HorizontalVelocity.magnitude < _break)
                {
                    _ctx.HorizontalVelocity = _ctx.InputVector * Vector3.Dot(_ctx.InputVector, _ctx.HorizontalVelocity);
                }
            }

            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.BaseSpeed)
            {
                float _acceleration = _ctx.Chp.Acceleration * _delta;
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chp.BaseSpeed);
            }
            else
            {
                float _Drag = _ctx.Chp.GroundDrag * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, _ctx.HorizontalVelocity.normalized * _ctx.Chp.BaseSpeed, _Drag);
            }

            if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chp.TurnDeceleration.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }

            float _turnStrength = _ctx.Chp.TurnStrengthCurve.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.InputVector * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }
        else
        {
            float _deceleration = _ctx.Chp.Deceleration * _delta;
            _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _deceleration);
        }
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void Slipment(float _delta)
    {
        _ctx.Skid = false;

        _slipState -= _delta;
        //  _ctx.HorizontalVelocity += _ctx.Chp.SlopeFactor * _delta * Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal);
        // _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void SlopePhysics(float _delta)
    {
        if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) <= FrameworkUtility.FloorAngle) return;

        float _slopeFactor = _ctx.Chp.SlopeFactor * _delta;
        _ctx.HorizontalVelocity += Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal) * _slopeFactor;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);

        Vector3 cameraForward = _ctx.InputRef.forward;

        float lookBackFactor = _ctx.Input.BackCameraInput.IsPressed() ? -1f : 1f;
        Vector3 targetForward = cameraForward * lookBackFactor;

        _ctx.CurrentMoveDirection = Vector3.Lerp(_ctx.CurrentMoveDirection, targetForward, Time.deltaTime * _ctx.Chp.LookBackTransitionSpeed);
        _ctx.CurrentMoveDirection = Vector3.ProjectOnPlane(_ctx.CurrentMoveDirection, _ctx.GroundNormal).normalized;

        Quaternion adjustedRotation = Quaternion.LookRotation(_ctx.CurrentMoveDirection, _ctx.GroundNormal);
        _ctx.InputVector = adjustedRotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void GroundSwitchConditions()
    {
        if (_ctx.RingDetector.TargetDetected && _ctx.Input.ReactionInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.LightSpeedDash);
        }

        if (_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Spindash);
            return;
        }

        if (_ctx.Input.CrouchInput.WasPressedThisFrame() && _ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinRollSpeed)
        {
            _ctx.MachineTransition(PlayerStates.Roll);
        }

        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.Jump();
        }
    }

    private void AirSwitchConditions()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }

    #endregion Util
}