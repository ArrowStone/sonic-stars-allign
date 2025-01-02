using UnityEngine;

public class Sonic_BounceState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _ddchargeTime;

    public Sonic_BounceState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _groundDetected = false;
        _ddchargeTime = 0;

        #endregion Misc

        #region Collision

        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);
        _ctx.TriggerCl.TriggerEnter += TriggerCheck;

        #endregion Collision

        #region Velocity

        _ctx.VerticalVelocity = _ctx.Gravity * _ctx.Chp.BounceSpeed;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.Gravity.normalized);
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

        AirSwitchConditions(_delta);
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
    }

    public void ExitState()
    {
        _ctx.TriggerCl.TriggerEnter -= TriggerCheck;
    }

    #region Util

    private bool GroundCheck()
    {
        var _check = _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, _ctx.Gravity.normalized) || _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, -_ctx.Gravity.normalized);
        _groundDetected = _check && Vector3.Dot(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) <= 0;

        if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity.normalized) > 0)
        {
            return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity.normalized) <= FrameworkUtility.SteepAngle && Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal).magnitude > _ctx.Chp.MinGroundStickSpeed;
        }
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity.normalized) <= FrameworkUtility.SlopeAngle;
    }

    private void Gravity(float _delta)
    {
        if (Vector3.Dot(_ctx.Velocity, _ctx.Gravity.normalized) >= _ctx.Chp.MaxBounceSpeed)
        {
            return;
        }
        _ctx.VerticalVelocity = Vector3.ClampMagnitude(_ctx.VerticalVelocity + _ctx.Chp.GravityForce * _delta * _ctx.Gravity, _ctx.Chp.MaxBounceSpeed);
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
            if (Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector) < _ctx.Chp.TurnDeviationCap)
            {
                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinBreakSpeed)
                {
                    //_ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _ctx.Chs.BreakStrengthAir * _delta);

                    float _acceleration = _ctx.Chp.AccelerationAir * _delta;
                    _ctx.HorizontalVelocity += (_acceleration * _ctx.InputVector);
                    _ctx.Skid = true;
                    return;
                }
                else
                {
                    _ctx.HorizontalVelocity = _ctx.InputVector * Vector3.Dot(_ctx.InputVector, _ctx.HorizontalVelocity);
                }
            }

            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.BaseSpeedAir)
            {
                float _acceleration = _ctx.Chp.AccelerationAir * _delta;
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chp.BaseSpeedAir);
            }

            if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chp.TurnDecelerationAir.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }

            float _turnStrength = _ctx.Chp.TurnStrengthCurveAir.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.InputVector * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }

        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
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

    private void TriggerCheck(Collider _cl)
    {
        if (_cl == _ctx.TriggerBuffer) return;

        Debug.Log("l");
        if (_cl.TryGetComponent(out Automation_ForceSpline _s))
        {
            _s.Execute(_ctx);
            _ctx.TriggerBuffer = _cl;
            return;
        }

        if (_cl.TryGetComponent(out Automation_GrindRail _gr))
        {
            _gr.Execute(_ctx, _ctx.Rb.position);
            _ctx.TriggerBuffer = _cl;
            return;
        }
    }

    private void GroundSwitchConditions()
    {
        if (_ctx.DropDashing)
        {
            float _ddForce = _ctx.Chp.DropDashOutput.Evaluate(_ddchargeTime);

            if (_ctx.InputVector.magnitude > 0)
            {
                _ctx.PlayerDirection = _ctx.InputVector;
            }
            if (Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).magnitude < _ddForce)
            {
                _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.PlayerDirection, _ctx.GroundNormal) * _ddForce;
            }

            _ctx.BounceCount = 0;
            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }
        Bounce();
    }

    private void AirSwitchConditions(float _delta)
    {
        DropDashCalculations(_delta);

        if (!_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.Dash();
        }
    }

    private void Bounce()
    {
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover, _ctx.SnapForce);
        float _bounceForce = Vector3.Dot(_ctx.Velocity, -_ctx.GroundCast.HitInfo.normal) * _ctx.Chp.BounceFactor.Evaluate(_ctx.BounceCount);
        if (_bounceForce > _ctx.Chp.MaxBounceHieght)
        {
            _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) + _ctx.GroundCast.HitInfo.normal * _ctx.Chp.MaxBounceHieght;
        }
        else
        {
            _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) + _ctx.GroundCast.HitInfo.normal * _bounceForce;
        }

        _ctx.BounceCount++;
        _ctx.MachineTransition(PlayerStates.Air);
    }

    private void DropDashCalculations(float _delta)
    {
        if (_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.IsPressed())
        {
            _ctx.DropDashing = true;
            Debug.Log(_ddchargeTime);
            _ddchargeTime += _delta;
        }
        else
        {
            _ctx.DropDashing = false;
        }
    }

    #endregion Util
}