using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class CamPoint_FollowSpline : MonoBehaviour, ICamPoint
{
    public CamBrain Brain;

    [Space]
    [Header("Parameters")]
    public Transform Target;

    public SplineContainer TargetSpline;
    public float followOffset;
    public float3 RotationOffset;

    [Space]
    public float DeadZone;

    public float2 YLimits;
    public float2 XLimits;
    public Vector2 MouseSensitivity;
    public Vector2 JoystickSensitivity;

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

    private float3 nearest;
    private float t;

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

        _rot.y += _inputValues.x * JoystickSensitivity.x * _delta;
        _rot.y = Mathf.Clamp(_rot.y, XLimits.x, XLimits.y);

        _rot.x += _inputValues.y * JoystickSensitivity.y * _delta;
        _rot.x = Mathf.Clamp(_rot.x, YLimits.x, YLimits.y);
    }

    public Vector3 UpdatePosition(float _delta)
    {
        SplineUtility.GetNearestPoint(TargetSpline.Spline, TargetSpline.transform.worldToLocalMatrix.MultiplyPoint(Target.position), out nearest, out t);
        t += followOffset;
        Vector3 output = TargetSpline.Spline.EvaluatePosition(t);
        output = TargetSpline.transform.localToWorldMatrix.MultiplyPoint(output);
        return output;
    }

    public Quaternion UpdateRotation(float _delta)
    {
        Debug.Log(TargetSpline.Spline.EvaluateTangent(t) + RotationOffset + " " + _rotation);
        return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(TargetSpline.Spline.EvaluateTangent(t) + RotationOffset), RotationSmoothTime * _delta);
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