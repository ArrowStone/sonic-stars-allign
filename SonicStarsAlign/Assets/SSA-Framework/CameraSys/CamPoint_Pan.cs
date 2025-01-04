using Unity.Mathematics;
using UnityEngine;

public class CamPoint_Pan : MonoBehaviour, ICamPoint
{
    public CamBrain Brain;

    [Space]
    [Header("Parameters")]
    public Transform Target;

    public Vector3 PointPosition;

    [Space]
    public float DeadZone;

    public float2 YLimits;
    public float2 XLimits;

    public Vector2 Sensitivity;

    [Space]
    public float RotationSmoothTime = 0.2f;

    [Space]
    public float RecenteringWait;

    public float RecenteringSpeed;

    #region Util

    private float _recenteringState;

    private Vector3 _cashedTargetPosition;

    private Vector2 _inputValues;

    private Vector2 _rot;

    private Quaternion addRot;

    #endregion Util

    public void OnEnter(CamBrain _brain)
    {
        Brain = _brain;
        _position = _brain.CashedPosition;

        _cashedTargetPosition = Target.position;
    }

    public void Execute(float _delta)
    {
        if (Target != null)
        {
            _cashedTargetPosition = Target.position;
        }
        if (Brain.Input != null)
        {
            InputHandling(_delta);
        }

        _position = UpdatePosition(_delta);
        _rotation = UpdateRotation(_delta);
    }

    public void OnExit()
    {
        Brain = null;
    }

    #region AdditionalFunctions

    private void InputHandling(float _delta)
    {
        if (_inputValues.magnitude < 0.1)
        {
            _recenteringState -= _delta;
            if (_recenteringState <= 0)
            {
                _rot.x = Mathf.LerpAngle(_rot.x, 0, RecenteringSpeed * _delta);
                _rot.y = Mathf.LerpAngle(_rot.y, 0, RecenteringSpeed * _delta);
            }
        }
        else
        {
            _recenteringState = RecenteringWait;
        }

        _inputValues = Vector2.ClampMagnitude(Brain.Input.CameraInput.ReadValue<Vector2>(), 1);

        _rot.y += _inputValues.x * Sensitivity.x * _delta;
        _rot.y = Mathf.Clamp(_rot.x, XLimits.x, XLimits.y);

        _rot.x += _inputValues.y * Sensitivity.y * _delta;
        _rot.x = Mathf.Clamp(_rot.x, YLimits.x, YLimits.y);
    }

    public Vector3 UpdatePosition(float _delta)
    {
        Vector3 _pos = PointPosition;
        return _pos;
    }

    public Quaternion UpdateRotation(float _delta)
    {
        return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(_cashedTargetPosition - _position), RotationSmoothTime * _delta);
    }

    #endregion AdditionalFunctions

    private Vector3 _position;

    public Vector3 Position()
    {
        return _position;
    }

    private Quaternion _rotation = Quaternion.identity;

    public Quaternion Rotation()
    {
        Quaternion _rotRot = new()
        {
            eulerAngles = new Vector3(_rot.x, _rot.y)
        };
        return  _rotRot * _rotation ;
    }
}