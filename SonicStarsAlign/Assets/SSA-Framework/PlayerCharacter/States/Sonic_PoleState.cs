using UnityEngine;

public class Sonic_PoleState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;

    public Sonic_PoleState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);
        _ctx.SplnHandler.Time = 0;
    }

    public void UpdateState()
    {
        _ctx.SplnHandler.Time += Time.deltaTime * _ctx.SplnHandler._ctxPole.TimeSpeed;
        if (_ctx.SplnHandler.Time > 1)
        {
            _ctx.SplnHandler.Time = 0;
        }

        PoleSwitchConditions();
    }

    public void FixedUpdateState()
    {
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.Jumping = false;
    }

    private void PoleSwitchConditions()
    {
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _ctx.SplnHandler._ctxPole.PoleLaunch(_ctx, _ctx.SplnHandler.Time);
        }
    }
}