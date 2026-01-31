using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

// Camera movement
public class CamPoint_NormalPlayer : MonoBehaviour, ICamPoint
{
    public CamBrain Brain;

    [Space]
    [Header("Parameters")]
    public Rigidbody Target;

    public float TargetDistance;

    public Vector3 Offset;

    [Space]
    public float DeadZone;

    public float2 YLimits;

    public Vector2 MouseSensitivity;
    public Vector2 JoystickSensitivity;

    [Space]
    public float SmoothRotationSpeed = 0.2f;

    public float MovementSmoothing;

    [Space]
    public float CameraRecenteringWait;

    public float YAxisRecenteringSpeed;
    public float XAxisRecenteringSpeed;

    public float BackCameraSpeed;

    #region Util

    private float _recenteringState;

    private Vector3 _moveVelocity = Vector3.zero;

    private Vector3 _cashedTargetPosition;

    private Vector2 _joystickInputValues;
    private Vector2 _mouseInputValues;

    private Vector2 _rot;

    #endregion Util

    public void OnEnter(CamBrain _brain)
    {
        Brain = _brain;
        _position = _brain.CashedTransform.Position;
        _rotation = _brain.CashedTransform.Rotation;
        _cashedTargetPosition = Target.GetComponentInChildren<Rigidbody>().position;
    }

    public void Execute(float _delta)
    {
        if (Target != null)
        {
            _cashedTargetPosition = Target.transform.position;
        }
        if (Brain.Input != null)
        {
            InputHandling(_delta);
        }

        
        //_position = SmoothMove(Brain, UpdatePosition(_delta), _delta);
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
        // looking behind where the player is facing
        if (Brain.Input.BackCameraInput.IsPressed())
        {
            _rot.y = Mathf.LerpAngle(_rot.y, Target.transform.eulerAngles.y + 180f, BackCameraSpeed * _delta);
        }
        else
        {
            if ((_joystickInputValues + _mouseInputValues).magnitude < 0.1f)
            {
                _recenteringState -= _delta;
                if (_recenteringState <= 0)
                {
                    _rot.x = Mathf.LerpAngle(_rot.x, 0, YAxisRecenteringSpeed * _delta);
                    _rot.y = Mathf.LerpAngle(_rot.y, Target.transform.eulerAngles.y, XAxisRecenteringSpeed * _delta);
                }

            }
            else
            {
                _recenteringState = CameraRecenteringWait;
            }

            // Screw joystick simulation we're going full SRB2
            //_inputValues = Vector2.ClampMagnitude(Brain.Input.CameraInput.ReadValue<Vector2>(), 1);
            _joystickInputValues = Brain.Input.CameraInputValues;
            _mouseInputValues = Mouse.current.delta.ReadValue(); // Have to do this so mouse input gets processed every dynamic update
            
            // Idk why x and y values are swapped but i dont wanna fix it
            _rot.y += _joystickInputValues.x * JoystickSensitivity.x * _delta;
            _rot.x -= _joystickInputValues.y * JoystickSensitivity.y * _delta;
            _rot.y += _mouseInputValues.x * MouseSensitivity.x * Time.timeScale; // Multiply by timescale so the camera wont rotate while paused
            _rot.x -= _mouseInputValues.y * MouseSensitivity.y * Time.timeScale;

            _rot.x = Mathf.Clamp(_rot.x, YLimits.x, YLimits.y);
        }
    }

    public Quaternion UpdateRotation(float _delta)
    {
        // Dunno why its here but it messes with using the mouse for rotation so it goes in the trash
        //return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(Target.position - _position), SmoothRotationSpeed * _delta);
        return Quaternion.LookRotation(Target.transform.position - _position);
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
        PosRot _transfrm = new()
        {
            Position = _position,
            Rotation = _rotation
        };
        return _transfrm;
    }
}