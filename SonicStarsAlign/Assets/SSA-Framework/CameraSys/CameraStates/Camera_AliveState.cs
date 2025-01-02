using UnityEngine;

public class Camera_AliveState : IState
{
    private readonly CamBrain _ctx;

    public Camera_AliveState(CamBrain _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.Point.OnEnter(_ctx);
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        _ctx.Point.Execute(_delta);
        _ctx.ApplyPoint();
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
    }
}