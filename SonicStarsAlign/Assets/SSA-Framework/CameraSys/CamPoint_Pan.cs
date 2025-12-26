using Unity.Mathematics;
using UnityEngine;

public class CamPoint_Pan : MonoBehaviour, ICamPoint
{
    public CamBrain Brain;

    [Space]
    [Header("Parameters")]
    public Transform Target;

    public Transform PointTransform;

    [Space]
    public float RotationSmoothTime = 0.2f;

    [Space]
    public float RecenteringWait;

    public float RecenteringSpeed;

    #region Util

    private Vector3 _cashedTargetPosition;


    private Vector2 _rot;

    #endregion Util

    public void OnEnter(CamBrain _brain)
    {
        Brain = _brain;
        _position = _brain.CashedTransform.Position;
        _rotation = _brain.CashedTransform.Rotation;
        _cashedTargetPosition = Target.position;
    }

    public void Execute(float _delta)
    {
        if (Target != null)
        {
            _cashedTargetPosition = Target.position;
        }

        _position = UpdatePosition(_delta);
        _rotation = UpdateRotation(_delta);
    }

    public void OnExit()
    {
        Brain = null;
    }

    #region AdditionalFunctions

    public Vector3 UpdatePosition(float _delta)
    {
        Vector3 _pos = PointTransform.position;
        return _pos;
    }

    public Quaternion UpdateRotation(float _delta)
    {
        return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(_cashedTargetPosition - _position), RotationSmoothTime * _delta);
    }

    #endregion AdditionalFunctions

    private Vector3 _position;

    private Quaternion _rotation = Quaternion.identity;

    public PosRot Transform()
    {
        Quaternion _rotRot = new()
        {
            eulerAngles = new Vector3(_rot.x, _rot.y)
        };
        PosRot _transfrm = new PosRot()
        {
            Position = _position,
            Rotation = _rotation * _rotRot,
        };
        return _transfrm;
    }
}