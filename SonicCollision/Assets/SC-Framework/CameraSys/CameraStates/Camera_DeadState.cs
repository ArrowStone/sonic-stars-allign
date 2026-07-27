public class Camera_DeadState : IState
{
    private readonly CamBrain _ctx;

    public Camera_DeadState(CamBrain _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
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