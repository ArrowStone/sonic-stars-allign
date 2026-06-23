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
        _ctx.ChangeKinematic(true);

        _triggerDetected = false;
        // Temporary
        initialGroundRayLength =  _ctx.GroundCast.DetectionDistance;
        _ctx.GroundCast.DetectionDistance = 10f;
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
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

        _ctx.ChangeKinematic(false);
    }

    private void Movement(float _delta)
    {
        _vel = (_pos - _ctx.transform.position) / _delta;

        _ctx.Physics_Snap(_pos);
        // Smoothen rotation
        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, Vector3.Lerp(_ctx.transform.up, _ctx.GroundNormal, 0.2f));
    }

    private void SplineApplication()
    {
        _pos = _ctx.SplnHandler.NewPosition();
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, -_ctx.GroundNormal);
        _ctx.VerticalVelocity = Vector3.Project(_vel, -_ctx.GroundNormal);
        _ctx.Physics_ApplyVelocity();

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