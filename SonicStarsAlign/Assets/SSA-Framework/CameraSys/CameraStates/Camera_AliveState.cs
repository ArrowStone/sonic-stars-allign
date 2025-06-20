using UnityEngine;

public class Camera_AliveState : IState
{
    private readonly CamBrain _ctx;
    private float _delta = 0f;

    public Camera_AliveState(CamBrain _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
    }

    public void UpdateState()
    {
        _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
    }

    public void LateUpdateState()
    {
        AliveMovement(_delta);
    }

    public void ExitState()
    {
        _ctx.Point.OnExit();
    }

    public void AliveMovement(float _delta)
    {
        _ctx.Point.Execute(_delta);
        _ctx.ApplyPoint();
    }
}