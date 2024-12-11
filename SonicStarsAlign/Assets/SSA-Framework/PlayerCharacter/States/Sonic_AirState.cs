using UnityEngine;

public class Sonic_AirState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;

    public Sonic_AirState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _groundDetected = false;

        #endregion Misc

        #region Collision

        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);

        #endregion Collision

        #region Velocity

        FrameworkUtility.SplitPlanarVector(_ctx.Velocity, -_ctx.Gravity.normalized, out var _v, out var _h);
        _ctx.VerticalVelocity = _v;
        _ctx.HorizontalVelocity = _h;
        _ctx.Physics_ApplyVelocity();

        InputRotations();

        #endregion Velocity
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        if (GroundCheck())
        {
            GroundSwitchConditions();
            return;
        }

        AirApplication(_delta);
        Gravity(_delta);
        Movement(_delta);
        AirSwitchConditions();
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
        var _check = _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, _ctx.Gravity.normalized) || _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, -_ctx.Gravity.normalized);
        _groundDetected = _check && Vector3.Dot(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) <= 0;

        if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity.normalized) > 0)
        {
            return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity.normalized) <= FrameworkUtility.SteepAngle && Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal).magnitude > _ctx.Chs.MinGroundStickSpeed;
        }
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity.normalized) <= FrameworkUtility.SlopeAngle;
    }

    private void Gravity(float _delta)
    {
        if (Vector3.Dot(_ctx.Velocity, _ctx.Gravity.normalized) >= _ctx.Chs.FallSpeedCap)
        {
            return;
        }

        if (_ctx.Jumping && _ctx.Input.JumpInput.WasReleasedThisFrame())
        {
            _ctx.Jumping = false;
            if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity) > _ctx.Chs.JumpCancelStrength)
            {
                _ctx.VerticalVelocity = _ctx.Chs.JumpCancelStrength * -_ctx.Gravity;
            }
            return;
        }
        _ctx.VerticalVelocity += _ctx.Chs.GravityForce * _delta * _ctx.Gravity;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(-_ctx.Gravity, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, -_ctx.Gravity) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, -_ctx.Gravity);

        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void Movement(float _delta)
    {
        _ctx.Skid = false;
        if (_ctx.InputVector.magnitude > 0.1)
        {
            if (Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector) < _ctx.Chs.TurnDeviationCap)
            {
                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chs.MinBreakSpeed)
                {
                    //_ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _ctx.Chs.BreakStrengthAir * _delta);

                    float _acceleration = _ctx.Chs.AccelerationCurveAir.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity, _ctx.InputVector)) * _delta;
                    _ctx.HorizontalVelocity += (_acceleration * _ctx.InputVector);
                    _ctx.Skid = true;
                    return;
                }
                else
                {
                    _ctx.HorizontalVelocity = _ctx.InputVector * Vector3.Dot(_ctx.InputVector, _ctx.HorizontalVelocity);
                }
            }

            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chs.BaseSpeedAir)
            {
                float _acceleration = _ctx.Chs.AccelerationCurveAir.Evaluate(_ctx.HorizontalVelocity.magnitude) * _delta;
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chs.BaseSpeedAir);
            }

            if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chs.TurnDecelerationAir.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }

            float _turnStrength = _ctx.Chs.TurnStrengthCurveAir.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.InputVector * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }
        else
        {
            float _deceleration = _ctx.Chs.DecelerationCurveAir.Evaluate(_ctx.HorizontalVelocity.magnitude) * _delta;
            _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _deceleration);
        }

        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chs.HardSpeedCap);
    }

    private void AirApplication(float _delta)
    {
        FrameworkUtility.SplitPlanarVector(_ctx.Velocity, -_ctx.Gravity.normalized, out var _h, out var _v);
        _ctx.VerticalVelocity = _v;
        _ctx.HorizontalVelocity = _h;

        InputRotations();
        AirRotation(_delta);
    }

    private void AirRotation(float _delta)
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

        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);
    }

    private void GroundSwitchConditions()
    {
        if (_ctx.Input.RollInput.IsPressed())
        {
            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chs.SpinDashInitSpeed)
            {
                _ctx.MachineTransition(PlayerStates.Spindash);
                return;
            }

            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }
        _ctx.MachineTransition(PlayerStates.Ground);
    }

    private void AirSwitchConditions()
    {
        if (_ctx.Input.RollInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.LightSpeedDash);
        }
    }

    #endregion Util
}