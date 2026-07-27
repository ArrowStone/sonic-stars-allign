public interface ICamPointStyle
{
    public void OnEnterPoint(CamBrain _cam);

    public void ExecutePoint(float _delta);

    public void OnExitPoint();
}