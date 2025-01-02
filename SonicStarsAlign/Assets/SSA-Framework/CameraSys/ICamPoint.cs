using UnityEngine;

public interface ICamPoint
{
    public Vector3 Position();

    public Quaternion Rotation();

    public void OnEnter(CamBrain _cam);

    public void Execute(float _delta);

    public void OnExit();
}