using UnityEngine;

public class Sonic_GrindState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;
    private Vector3 _pos;
    private Vector3 _norm;
    private bool _collided;
    private float _dotP;

    public Sonic_GrindState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);
        _vel = Vector3.Project(_ctx.Velocity, _ctx.SplnHandler.SplineTangent());
        RailApplication();
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        _ctx.SplnHandler.SplineMove(_delta);
        if (_ctx.SplnHandler.Active)
        {
            RailApplication();
            if (!CollisionD(_delta))
            {
                Movement(_delta);
                Rotation();
                SlopePhysics(_delta);
            }
        }
        RailSwitchConditions();
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity);
        _ctx.Physics_ApplyVelocity();
        _ctx.SplnHandler.Clear();

        _vel = Vector3.zero;
    }

    #region Util

    private void RailApplication()
    {
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, _ctx.GroundNormal);
        _ctx.VerticalVelocity = Vector3.Project(_vel, _ctx.GroundNormal);
        _pos = _ctx.SplnHandler.NewPosition();
        _norm = _ctx.SplnHandler.SplineNormal();
        _ctx.GroundNormal = _norm;
    }

    private void Movement(float _delta)
    {
        _vel = (_pos - _ctx.Rb.position) / _delta;
        _ctx.Physics_Snap(_pos);
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

    private void Rotation()
    {
        if (_vel.magnitude > 0.1f)
        {
            _ctx.PlayerDirection = _vel.normalized;
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

    public void RailSwitchConditions()
    {
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
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

    #endregion Util
}