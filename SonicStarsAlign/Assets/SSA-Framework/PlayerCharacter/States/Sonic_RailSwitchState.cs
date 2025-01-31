using UnityEngine;

public class Sonic_RailSwitchState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;
    private float _t;

    public Sonic_RailSwitchState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        if (_ctx.SplnHandler.SwitchDir)
        {
            _vel = _ctx.Velocity + _ctx.Chp.RailSwitchSpeed * _ctx.transform.right;
        }
        else
        {
            _vel = _ctx.Velocity + _ctx.Chp.RailSwitchSpeed * -_ctx.transform.right;
        }

        _ctx.ChangeKinematic(true);

        _ctx.PlayerDirection = _vel.normalized;
        _ctx.GroundNormal = -_ctx.Gravity;
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        if (ContinueRailSwitch(_delta))
        {
            RailSwitchMovement(_delta);
        }
        else
        {
            ExitConditions();
        }
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Velocity = _vel;
    }

    private void RailSwitchMovement(float _delta)
    {
        _ctx.Physics_Snap(_ctx.Rb.position + _vel * _delta);
    }

    private bool ContinueRailSwitch(float _delta)
    {
        if (_t < _ctx.Chp.RailSwitchDuration)
        {
            _t += _delta;
            return false;
        }
        return true;
    }

    public void RailSwitchConditions()
    {
        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Bounce);
        }
    }

    private void ExitConditions()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }
}