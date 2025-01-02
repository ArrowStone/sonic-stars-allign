using System;
using Unity.Mathematics;
using UnityEngine;

public class CamPoint_NormalPlayer : MonoBehaviour, ICamPoint
{
    public CamBrain Brain;

    [Space]
    [Header("Parameters")]
    public Transform Target;

    public float TargetDistance;

    public Vector3 Offset;

    [Space]
    public float DeadZone;

    public float2 YLimits;

    public Vector2 Sensitivity;

    [Space]
    public float RotationSmoothTime = 0.2f;

    public float MovementSmoothing;

    public float DistanceSmoothing;

    [Space]
    public float YAxisRecenteringWait;

    public float YAxisRecenteringSpeed;

    #region Util

    private float _recenteringState;

    private Vector3 _smoothVelocity = Vector3.zero;

    private Vector3 _moveVelocity = Vector3.zero;

    private float _distanceVelocity = 0;

    private Vector3 _cashedTargetPosition;

    private Vector2 _inputValues;

    private Vector2 _rot;

    private float CurentDistance => Vector3.Distance(_cashedTargetPosition, _position);

    #endregion Util

    public void OnEnter(CamBrain _brain)
    {
        _cashedTargetPosition = Target.position;
        Brain = _brain;
        _position = _brain.CashedPosition;
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

        _rotation = UpdateRotation(_delta);
        _position = SmoothMove(Brain, UpdatePosition(_rotation, _delta), _delta);
    }

    public void OnExit()
    {
        throw new NotImplementedException();
    }

    #region AdditionalFunctions

    private void InputHandling(float _delta)
    {
        if (_inputValues.magnitude < 0.1)
        {
            _recenteringState -= _delta;
            if (_recenteringState <= 0)
            {
                _rot.x = Mathf.LerpAngle(_rot.x, 0, YAxisRecenteringSpeed * _delta);
            }
        }
        else
        {
            _recenteringState = YAxisRecenteringWait;
        }

        _inputValues = Vector2.ClampMagnitude(Brain.Input.CameraInput.ReadValue<Vector2>(), 1);

        _rot.y += _inputValues.x * Sensitivity.x * _delta;
        _rot.x += _inputValues.y * Sensitivity.y * _delta;

        _rot.x = Mathf.Clamp(_rot.x, YLimits.x, YLimits.y);
    }

    public Quaternion UpdateRotation(float _delta)
    {
        Quaternion _oldRot = new()
        {
            eulerAngles = Vector3.SmoothDamp(_rot, new Vector3(_rot.x, _rot.y), ref _smoothVelocity, RotationSmoothTime, Mathf.Infinity, _delta)
        };
        return _oldRot;
    }

    public Vector3 UpdatePosition(Quaternion _rotate, float _delta)
    {
        return _cashedTargetPosition + Offset + (Mathf.SmoothDamp(CurentDistance, TargetDistance, ref _distanceVelocity, DistanceSmoothing, Mathf.Infinity, _delta) * (_rotate * Vector3.forward));
    }

    public Vector3 SmoothMove(CamBrain camBrain, Vector3 Position, float _delta)
    {
        return Vector3.SmoothDamp(camBrain.CashedPosition, Position, ref _moveVelocity, MovementSmoothing, Mathf.Infinity, _delta);
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
        return _rotation;
    }
}