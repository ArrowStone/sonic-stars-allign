using UnityEngine;

public class Sonic_RollState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;

    public Sonic_RollState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _groundDetected = true;
        _ctx.Anim.SetInteger("State", 1);
        _ctx.ModelManager.EnterBall(true);


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
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.HomingCheck();

        if (!GroundCheck())
        {
            AirSwitchConditions();
            return;
        }

        GroundApplication(_delta);
        SlopePhysics(_delta);
        Movement(_delta);

        GroundSwitchConditions();
        _ctx.Physics_ApplyVelocity();
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
        _groundDetected = _ctx.GroundCast.Execute(_ctx.Rb.transform.position, -_ctx.GroundNormal);
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, _ctx.GroundNormal) <= _ctx.Chp.MaxGroundDeviation;
    }

    private void GroundApplication(float _delta)
    {
        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized * _ctx.Velocity.magnitude;
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover);

        _ctx.ModelManager.ballRollSpeed = _ctx.PlayerRunningSpeed;

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

        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, Vector3.Lerp(_ctx.transform.up, _ctx.GroundCast.HitInfo.normal, _delta * _ctx.Chp.RotationSmoothingSpeed));
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
                    _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _ctx.Chp.BreakStrength * _delta);
                    _ctx.Skid = true;
                    return;
                }
                else
                {
                    _ctx.HorizontalVelocity = _ctx.PlayerDirection * Vector3.Dot(_ctx.PlayerDirection, _ctx.HorizontalVelocity);
                }
            }

            if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chp.TurnDeceleration.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }

            float _turnStrength = _ctx.Chp.TurnStrengthCurve.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.PlayerDirection * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }

        float _deceleration = _ctx.Chp.RollDeceleration * _delta;
        _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _deceleration);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void SlopePhysics(float _delta)
    {
        //if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) <= FrameworkUtility.FloorAngle) return;

        float _slopeFactor = _ctx.Chp.SlopeFactorRoll * _delta;
        if (Vector3.Dot(_ctx.Gravity, _ctx.HorizontalVelocity.normalized) >= 0)
        {
            _slopeFactor = _ctx.Chp.SlopeFactorRollDown * _delta;
        }

        _ctx.HorizontalVelocity += Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal) * _slopeFactor;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);

        //  Quaternion cameraRotation = Quaternion.Euler(0, StateMachine.Camera.eulerAngles.y, 0);
        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void GroundSwitchConditions()
    {
        if (_ctx.RingDetector.TargetDetected && _ctx.Input.LightDashInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.LightSpeedDash);
        }

        if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.MinRollSpeed || _ctx.Input.CrouchInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Ground);
            _ctx.ModelManager.ExitBall();
        }

        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Spindash);
            return;
        }

        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.Jump();
        }
    }

    private void AirSwitchConditions()
    {
        _ctx.ModelManager.ExitBall();
        _ctx.MachineTransition(PlayerStates.Air);
    }

    #endregion Util
}