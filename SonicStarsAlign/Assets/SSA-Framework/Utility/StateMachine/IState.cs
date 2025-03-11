public interface IState
{
    public void EnterState();

    public void UpdateState();

    public void FixedUpdateState();

    public void LateUpdateState();

    public void ExitState();
}