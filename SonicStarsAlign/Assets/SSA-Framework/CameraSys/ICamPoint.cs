public interface ICamPoint
{
    public PosRot Transform();

    public void OnEnter(CamBrain _cam);

    public void Execute(float _delta);

    public void OnExit();
}