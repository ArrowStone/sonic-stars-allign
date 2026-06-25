using UnityEngine;

public class Sonic_WinState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;

    public Sonic_WinState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.HorizontalVelocity = Vector3.zero;
        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
    }
}