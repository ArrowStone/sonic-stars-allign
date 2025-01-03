using UnityEngine;

public class Sonic_AirState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _ddchargeTime;

    public Sonic_AirState(Sonic_PlayerStateMachine _machine)
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
        _ctx.PlayerDirection = _ctx.Rb.transform.forward;
        _ctx.TriggerCl.TriggerEnter += TriggerCheck;
        _ctx.TriggerCl.TriggerExit += TriggerDCheck;
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
        DropDashCalculations(_delta);

        AirSwitchConditions();
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        _ctx.RingCheck();
        _ctx.HomingCheck();
    }

    public void ExitState()
    {
        _ctx.TriggerCl.TriggerEnter -= TriggerCheck;
        _ctx.TriggerCl.TriggerExit -= TriggerDCheck;

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
        if (Vector3.Dot(_ctx.Velocity, _ctx.Gravity.normalized) >= _ctx.Chp.FallVelCap)
        {
            return;
        }

        if (_ctx.Jumping && _ctx.Input.JumpInput.WasReleasedThisFrame())
        {
            _ctx.Jumping = false;
            if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity) > _ctx.Chp.JumpCancel)
            {
                _ctx.VerticalVelocity = _ctx.Chp.JumpCancel * -_ctx.Gravity;
            }
            return;
        }
        _ctx.VerticalVelocity = Vector3.ClampMagnitude(_ctx.VerticalVelocity + _ctx.Chp.GravityForce * _delta * _ctx.Gravity, _ctx.Chp.FallVelCap);
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
                float _acceleration = _ctx.Chp.AccelerationAir * _delta;
                _ctx.HorizontalVelocity += _acceleration * _ctx.InputVector;

                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinBreakSpeed)
                {
                    _ctx.Skid = true;
                    return;
                }
                else if (_ctx.HorizontalVelocity.magnitude < _acceleration)
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
        AirDrag(_delta);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void AirDrag(float _delta)
    {
        if (Vector3.Dot(_ctx.VerticalVelocity, -_ctx.Gravity.normalized) <= _ctx.Chp.JumpCancel) return;
        if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.BaseSpeedAir)
        {
            float _airDrag = _ctx.Chp.AirDrag * _delta;
            _ctx.HorizontalVelocity = _ctx.HorizontalVelocity.normalized * Mathf.Lerp(_ctx.HorizontalVelocity.magnitude, _ctx.Chp.BaseSpeedAir, _airDrag);
        }
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

    private void TriggerDCheck(Collider _)
    {
        _ctx.TriggerBuffer = null;
    }

    private void GroundSwitchConditions()
    {
        _ctx.AirDashes = 1;
        _ctx.BounceCount = 0;
        if (_ctx.DropDashing)
        {
            float _ddForce = _ctx.Chp.DropDashOutput.Evaluate(_ddchargeTime);

            if (_ctx.InputVector.magnitude > 0)
            {
                _ctx.PlayerDirection = _ctx.InputVector;
            }
            if (Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).magnitude < _ddForce)
            {
                _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.InputVector, _ctx.GroundNormal) * _ddForce;
            }

            _ctx.BounceCount = 0;
            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }

        if (_ctx.Input.CrouchInput.IsPressed() && _ctx.HorizontalVelocity.magnitude > _ctx.Chp.SpinDashInitSpeed)
        {
            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }
        _ctx.MachineTransition(PlayerStates.Ground);
    }

    private void AirSwitchConditions()
    {
        if(_ctx.HomingTargetDetector.TargetDetected && _ctx.Input.AttackInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.HomingAttack);
        }
        if (_ctx.RingDetector.TargetDetected && _ctx.Input.ReactionInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.LightSpeedDash);
        }
        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Bounce);
        }
        if (!_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.WasPressedThisFrame() && _ctx.AirDashes > 0)
        {
            _ctx.AirDashes--;
            _ctx.Dash();
        }
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