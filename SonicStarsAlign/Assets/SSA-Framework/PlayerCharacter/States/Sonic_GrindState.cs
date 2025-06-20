using UnityEngine;

public class Sonic_GrindState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private bool _collided;
    private Vector3 _difference;
    private float _dotP;
    private Vector3 _norm;
    private Vector3 _pos;
    private Vector3 _vel;

    public Sonic_GrindState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);
        _difference = Vector3.Project(_ctx.Velocity, _ctx.SplnHandler.SplineTangent());
        _ctx.VerticalVelocity = Vector3.zero;

        RailApplication();
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Physics_ApplyVelocity();
        _ctx.SplnHandler.Clear();

        _vel = Vector3.zero;
    }

    public void FixedUpdateState()
    {
    }

    public void LateUpdateState()
    {
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        _ctx.SplnHandler.SplineMove(_delta);
        if (_ctx.SplnHandler.Active)
        {
            RailApplication();
            InputRotations();
            if (!CollisionD(_delta))
            {
                Movement(_delta);
                Rotation();
                SlopePhysics(_delta);
            }
        }
        _ctx.RailCheck();
        RailSwitchConditions();
    }

    #region Util

    public void RailSwitchConditions()
    {
        float _tilt = Vector3.SignedAngle(_ctx.PlayerDirection, _ctx.InputVector, _ctx.GroundNormal);
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            Automation_GrindRail _grail;
            if (_tilt < -_ctx.Chp.RailSwitchDeadZone && _ctx.RailDetectorL.TargetDetected)
            {
                if (_ctx.RailDetectorL.TargetOutput.TryGetComponent(out _grail))
                {
                    _ctx.SplnHandler.SwitchDir = _grail;
                    _ctx.MachineTransition(PlayerStates.RailSwitch);
                    Debug.Log("L");
                    return;
                }
            }
            if (_tilt > _ctx.Chp.RailSwitchDeadZone && _ctx.RailDetectorR.TargetDetected)
            {
                if (_ctx.RailDetectorR.TargetOutput.TryGetComponent(out _grail))
                {
                    _ctx.SplnHandler.SwitchDir = _grail;
                    _ctx.MachineTransition(PlayerStates.RailSwitch);
                    Debug.Log("R");
                    return;
                }
            }
            _ctx.Jump();
        }
        if (!_ctx.SplnHandler.Active)
        {
            if (_ctx.GroundCast.Execute(_ctx.transform.position, -_ctx.GroundNormal))
            {
                _ctx.MachineTransition(PlayerStates.Ground);
                return;
            }
            _ctx.MachineTransition(PlayerStates.Air);
        }
    }

    private bool CollisionD(float _delta)
    {
        _collided = _ctx.Physics_Sweep(_ctx.SplnHandler.NewPosition(), out _);

        if (_collided)
        {
            _ctx.SplnHandler.SpeedMultiplier = _ctx.SplnHandler.Forward ? -_ctx.Chp.RailCollisionBounce : _ctx.Chp.RailCollisionBounce;
            _ctx.SplnHandler.SplineMove(_delta);
        }
        return _collided;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);

        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void Movement(float _delta)
    {
        _vel = _difference / _delta;
        _ctx.Physics_Snap(_pos);
    }

    private void RailApplication()
    {
        _norm = _ctx.SplnHandler.SplineNormal();
        _ctx.GroundNormal = _norm;

        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, _ctx.GroundNormal);
        _pos = _ctx.SplnHandler.NewPosition();
        _difference = _pos - _ctx.Rb.position;
    }

    private void Rotation()
    {
        if (_difference.magnitude >= _ctx.Rb.sleepThreshold)
        {
            _ctx.PlayerDirection = _difference.normalized;
        }
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private void SlopePhysics(float _delta)
    {
        if (_collided)
        {
            return;
        }

        _dotP = Vector3.Dot(_ctx.SplnHandler.SplineTangent().normalized, _ctx.Gravity);
        if (_ctx.Input.CrouchInput.IsPressed())
        {
            if ((_ctx.SplnHandler.Forward && _dotP > 0) || (!_ctx.SplnHandler.Forward && _dotP < 0))
            {
                _ctx.SplnHandler.SpeedMultiplier += _dotP * _ctx.Chp.RailCrouchInfluence * _delta;
            }
            else
            {
                _ctx.SplnHandler.SpeedMultiplier += _dotP * _ctx.Chp.RailSlopeInfluence * _delta;
            }
            return;
        }
        _ctx.SplnHandler.SpeedMultiplier += _dotP * _ctx.Chp.RailSlopeInfluence * _delta;
        _ctx.SplnHandler.SpeedMultiplier = Mathf.Clamp(_ctx.SplnHandler.SpeedMultiplier, -_ctx.Chp.RailSpeedCap, _ctx.Chp.RailSpeedCap);
    }

    #endregion Util
}