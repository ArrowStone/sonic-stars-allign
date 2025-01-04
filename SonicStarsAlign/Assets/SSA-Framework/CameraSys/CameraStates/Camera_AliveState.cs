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

    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        AliveMovement(_delta);
    }

    public void FixedUpdateState()
    {
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