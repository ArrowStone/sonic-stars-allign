using UnityEngine;

public class Sonic_LinearAutomationState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;
    private Vector3 _pos;
    private bool _triggerDetected;

    public Sonic_LinearAutomationState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);
        _ctx.GroundNormal = -_ctx.Gravity;
        _triggerDetected = false;

        _ctx.ModelManager.ExitBall();
        _ctx.Anim.SetInteger("State", 5);
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.HomingCheck();

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
        _ctx.ChangeKinematic(false);
        _ctx.Physics_ApplyVelocity();

        _vel = Vector3.zero;

        if (_triggerDetected) return;
        _ctx.SplnHandler.Clear();
    }

    private void Movement(float _delta)
    {
        _vel = (_pos - _ctx.transform.position) / _delta;

        _ctx.Physics_Snap(_pos);
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private void SplineApplication()
    {
        _pos = _ctx.SplnHandler.NewPosition();
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, -_ctx.Gravity);
        _ctx.VerticalVelocity = Vector3.Project(_vel, -_ctx.Gravity);

        if (Vector3.ProjectOnPlane(_vel, -_ctx.Gravity).magnitude > 0.1f)
        {
            _ctx.PlayerDirection = _vel.normalized;
        }
    }

    private void AutomationSwitchConditions()
    {
        if (_ctx.SplnHandler.Loose)
        {
            if (_ctx.Input.BounceInput.WasPressedThisFrame())
            {
                _ctx.MachineTransition(PlayerStates.Bounce);
            }
        }
        if (!_ctx.SplnHandler.Active)
        {
            _ctx.MachineTransition(PlayerStates.Air);
            return;
        }
    }
}