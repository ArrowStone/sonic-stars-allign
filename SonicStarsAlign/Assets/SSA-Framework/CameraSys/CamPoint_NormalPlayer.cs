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
    public float SmoothRotaionSpeed = 0.2f;

    public float MovementSmoothing;

    [Space]
    public float YAxisRecenteringWait;

    public float YAxisRecenteringSpeed;

    public float BackCameraSpeed;

    #region Util

    private float _recenteringState;

    private Vector3 _moveVelocity = Vector3.zero;

    private Vector3 _cashedTargetPosition;

    private Vector2 _inputValues;

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
        if (Brain.Input != null)
        {
            InputHandling(_delta);
        }

        _position = SmoothMove(Brain, UpdatePosition(_delta), _delta);
        _rotation = UpdateRotation(_delta);
    }

    public void OnExit()
    {
        Brain = null;
    }

    #region AdditionalFunctions

    private void InputHandling(float _delta)
    {
        // looking behind where the player is facing
        if (Brain.Input.BackCameraInput.IsPressed())
        {
            _rot.y = Mathf.LerpAngle(_rot.y, Target.eulerAngles.y + 180f, BackCameraSpeed * _delta);
        }
        else
        {
            if (_inputValues.magnitude < 0.1)
            {
                _recenteringState -= _delta;
                if (_recenteringState <= 0)
                {
                    _rot.x = Mathf.LerpAngle(_rot.x, 0, YAxisRecenteringSpeed * _delta);
                    _rot.y = Mathf.LerpAngle(_rot.y, Target.eulerAngles.y, YAxisRecenteringSpeed * _delta);
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
    }


    public Quaternion UpdateRotation(float _delta)
    {
        return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(Target.position - _position), SmoothRotaionSpeed * _delta);
    }

    public Vector3 UpdatePosition(float _delta)
    {
        Quaternion _posRot = new()
        {
            eulerAngles = new Vector3(_rot.x, _rot.y)
        };
        return _cashedTargetPosition + Offset + (_posRot * (Vector3.forward * TargetDistance));
    }

    public Vector3 SmoothMove(CamBrain camBrain, Vector3 Position, float _delta)
    {
        return Vector3.SmoothDamp(camBrain.CashedTransform.Position, Position, ref _moveVelocity, MovementSmoothing, Mathf.Infinity, _delta);
    }

    #endregion AdditionalFunctions

    private Vector3 _position;
    private Quaternion _rotation = Quaternion.identity;

    public PosRot Transform()
    {
        PosRot _transfrm = new PosRot()
        {
            Position = _position,
            Rotation = _rotation
        };
        return _transfrm;
    }
}