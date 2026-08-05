using UnityEngine;

public class Sonic_SurfAirState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _ddchargeTime;
    private float _airDragTime;
    private float _reactionInputTimer;

    public Sonic_SurfAirState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _groundDetected = false;
        _ddchargeTime = 0;
        _airDragTime = 0;
        _reactionInputTimer = 0;
        _ctx.doneAirRotation = false;
        _ctx.fakeNormal = _ctx.GroundNormal;
        _ctx.ModelManager.ballRollSpeed = 40f;

        _ctx.Anim.SetInteger("State", 2);
        _ctx.ModelManager.ExitBall();

        #endregion Misc

        #region Collision

        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.fakeNormal);
        _ctx.PlayerDirection = _ctx.transform.forward;


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
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        _ctx.HomingCheck();

        if (GroundCheck())
        {
            GroundSwitchConditions();
            return;
        }

        AirApplication(_delta);
        Gravity(_delta);
        Movement(_delta);
        RotateTowardVertical(_delta);

        _ctx.Physics_ApplyVelocity();

        AirSwitchConditions();
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ModelManager.ExitBall();
        _ctx.InBall = false;
        _ctx.LowGravity = false;
    }

    #region Util

    private bool GroundCheck()
    {
        var _check = _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, _ctx.Gravity.normalized);
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

        if (_ctx.LowGravity && _ctx.Input.JumpInput.WasReleasedThisFrame())
        {
            _ctx.LowGravity = false;
            //_ctx.InBall = false;
            if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity) > _ctx.Chp.JumpCancel)
            {
                _ctx.VerticalVelocity = _ctx.Chp.JumpCancel * -_ctx.Gravity;
            }
            return;
        }

        float gravity = _ctx.Chp.GravityForce;
        if (_ctx.LowGravity) gravity *= _ctx.Chp.JumpGravityScale;

        _ctx.VerticalVelocity = Vector3.ClampMagnitude(_ctx.VerticalVelocity + gravity * _delta * _ctx.Gravity, _ctx.Chp.FallVelCap);
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
        float _turnStrength = _ctx.ChrTurnSurf.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
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
                float _acceleration = _ctx.Chp.BreakStrengthAir * _delta;
                _ctx.HorizontalVelocity += _acceleration * _ctx.InputVector;

                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinBreakSpeed)
                {
                    _ctx.Skid = true;
                    return;
                }
                else if (_ctx.HorizontalVelocity.magnitude < _acceleration)
                {
                    _ctx.HorizontalVelocity = _ctx.PlayerDirection * Vector3.Dot(_ctx.InputVector, _ctx.HorizontalVelocity);
                }
            }

            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.BaseSpeedAir)
            {
                float _acceleration = _ctx.Chp.AccelerationAir * _delta;
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chp.BaseSpeedAir);
            }

            /*if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chp.TurnDecelerationAir.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }*/

            float _turnStrength = _ctx.ChrTurnSurf.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.PlayerDirection * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }

        AirDrag(_delta);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void AirDrag(float _delta)
    {
        if (Vector3.Dot(_ctx.VerticalVelocity, -_ctx.Gravity.normalized) <= _ctx.Chp.JumpCancel)
        {
            _airDragTime = 0;
            return;
        }

        if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.BaseSpeed)
        {
            _airDragTime += _delta;
            _ctx.HorizontalVelocity = _ctx.HorizontalVelocity.normalized * Mathf.Lerp(_ctx.HorizontalVelocity.magnitude, _ctx.Chp.BaseSpeed, _ctx.Chp.AirDrag.Evaluate(_airDragTime));
        }
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
        _ctx.MachineTransition(PlayerStates.Surf);
    }

    private void AirSwitchConditions()
    {
        if (_ctx.Input.ReactionInput.WasPressedThisFrame())
        {
            // Unmounting
            _ctx.SurfedProp.Unmount(_ctx);
            _ctx.MachineTransition(PlayerStates.Air);
        }

    }

    void RotateTowardVertical(float delta)
    {
        if (!_ctx.doneAirRotation && Vector3.Dot(_ctx.Gravity.normalized, _ctx.fakeNormal) < -0.995f)
        {
            _ctx.doneAirRotation = true;
            _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);
        }
        if (!_ctx.doneAirRotation)
        {
            _ctx.fakeNormal = Vector3.RotateTowards(_ctx.fakeNormal, -_ctx.Gravity.normalized, delta * _ctx.airRotationSpeed, 0f);
            _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.fakeNormal);
        }
    }
    #endregion Util
}