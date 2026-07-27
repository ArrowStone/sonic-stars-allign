public class Enemy_BaseState : IState
{
    private readonly Enemy_MovementStateMachine _etx;

    public Enemy_BaseState(Enemy_MovementStateMachine _machine)
    {
        _etx = _machine;
    }

    public void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public void FixedUpdateState()
    {
        throw new System.NotImplementedException();
    }

    public void LateUpdateState()
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }
}
