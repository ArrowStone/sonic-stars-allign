using UnityEngine;

// Used for automation when player needs to follow a predetermined path on the ground (i. e. loops)
public class Sonic_PathFollowState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;
    private Vector3 _pos;
    private bool _triggerDetected;
    private float initialGroundRayLength;

    public Sonic_PathFollowState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _triggerDetected = false;
        // Temporary
        initialGroundRayLength =  _ctx.GroundCast.DetectionDistance;
        _ctx.GroundCast.DetectionDistance = 100f;

        _pos = _ctx.SplnHandler.NewPosition();

        if(_ctx.GroundCast.Execute( _pos,
                                    -_ctx.SplnHandler.SplineNormal()))
        {
            _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
            _pos = _ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover;

            _ctx.PlayerDirection = _ctx.SplnHandler.SplineTangent();

            _ctx.Physics_Snap(_pos);
            _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
        }
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;


        _ctx.SplnHandler.SplineMove(_delta);
        if (_ctx.SplnHandler.Active)
        {
            SplineApplication();
            Movement(_delta);
        }

        AutomationSwitchConditions();
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.Physics_ApplyVelocity();

        _vel = Vector3.zero;

        if (_triggerDetected) return;
        _ctx.SplnHandler.Clear();

        _ctx.GroundCast.DetectionDistance = initialGroundRayLength;
    }

    private void Movement(float _delta)
    {
        //_vel = (_pos - _ctx.transform.position) / _delta;
        _vel = _ctx.PlayerDirection * _ctx.SplnHandler.SpeedMultiplier;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, -_ctx.GroundNormal);
        _ctx.VerticalVelocity = Vector3.Project(_vel, -_ctx.GroundNormal);
        _ctx.Physics_ApplyVelocity();

        _ctx.Physics_Snap(_pos);
        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private void SplineApplication()
    {
        _pos = _ctx.SplnHandler.NewPosition();

        if(_ctx.GroundCast.Execute( _pos + _ctx.SplnHandler.SplineNormal() * 5f,
                                    -_ctx.SplnHandler.SplineNormal()))
        {
            _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
            _pos = _ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover;

            _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.SplnHandler.SplineTangent(), _ctx.GroundNormal).normalized;
        }
        else _ctx.MachineTransition(PlayerStates.Air);
    }

    private void AutomationSwitchConditions()
    {
        if (_ctx.SplnHandler.Loose)
        {
            if (_ctx.Input.JumpInput.WasPressedThisFrame())
            {
                _ctx.Jump();
            }
        }
        if (!_ctx.SplnHandler.Active)
        {
            _ctx.MachineTransition(PlayerStates.Air);
            return;
        }
    }
}