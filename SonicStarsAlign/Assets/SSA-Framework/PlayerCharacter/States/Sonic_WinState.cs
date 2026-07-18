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
        _ctx.Anim.SetInteger("State", 0);
        _ctx.GroundCast.Execute(_ctx.transform.position, -_ctx.GroundNormal);
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover);
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