using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

// Camera movement
public class CamPoint_NormalPlayer : MonoBehaviour, ICamPoint
{

        [SerializeField,TextSpace("This component handles all default behaviour for the camera when it is locked on the character in normal gameplay. Control, effects, follow, etc.")]
        bool DummyText;

        public CamBrain Brain;

        [Header("Parameters")]

        #region Cinemachine Data

        [Header("Cinemachine Camera Control")]
        [TextSpace("The below composers will overwrite the provided Cinemachine Position Composer, depending if the camera is behind or infront of the character.")]
        public bool DummyText2;

        [ColourIfNull(0.8f, 0.1f, 0.1f, 1f)]
        public CinemachineRotationComposer ComposerToOverwrite;
        public CinemachineOrbitalFollow OrbitalToOverwrite;
        public CineCameraData CineCameraWhenBehind;
        public CineCameraData CineCameraWhenInFront;

        private float _LerpBehindToInFront;

        [System.Serializable]
        public class CineCameraData
        {
                public float cameraDistance = 3;
                public float deadZoneDepth = 3;
                public Vector2 screenPosition;

                [HideInInspector]
                public bool deadZone = true;
                [DrawTickBoxBefore("deadZone")]
                public Vector2 deadZoneSize = new Vector2(0.2f,0.1f);

                [HideInInspector]
                public bool hardLimits = true;
                [DrawTickBoxBefore("hardLimits")]
                public Vector2 hardLimitsSize = new Vector2(0.8f,0.6f);
                [DrawTickBoxBefore("hardLimits")]
                public Vector2 hardLimitsOffset = new Vector2(0f,0f);

                public bool centerOnActivate = true;

                [Header("Target Tracking")]

                public Vector3 targetOffset = new Vector3();
                public Vector3 damping;
                public bool lookahead = true;
                [Range(0,1)]
                public float time;
                [Range(0,1)]
                public float smoothing;
                public bool ignoreY;
        }

        public void ApplyComposerDataToComposer (float lerpAmount) {
                // Framing / distance
                ComposerToOverwrite.Composition.ScreenPosition = Vector2.Lerp(CineCameraWhenBehind.screenPosition, CineCameraWhenInFront.screenPosition, lerpAmount);

                // Dead zone
                ComposerToOverwrite.Composition.DeadZone.Enabled = lerpAmount < 0.5f ? CineCameraWhenBehind.deadZone : CineCameraWhenInFront.deadZone;
                ComposerToOverwrite.Composition.DeadZone.Size = Vector2.Lerp(CineCameraWhenBehind.deadZoneSize, CineCameraWhenInFront.deadZoneSize, lerpAmount);

                // Hard limits
                ComposerToOverwrite.Composition.HardLimits.Enabled = lerpAmount < 0.5f ? CineCameraWhenBehind.hardLimits : CineCameraWhenInFront.hardLimits;
                ComposerToOverwrite.Composition.HardLimits.Size = Vector2.Lerp(CineCameraWhenBehind.hardLimitsSize, CineCameraWhenInFront.hardLimitsSize, lerpAmount);
                ComposerToOverwrite.Composition.HardLimits.Offset = Vector2.Lerp(CineCameraWhenBehind.hardLimitsOffset, CineCameraWhenInFront.hardLimitsOffset, lerpAmount);

                // Activation behavior
                ComposerToOverwrite.CenterOnActivate = lerpAmount < 0.5f ? CineCameraWhenBehind.centerOnActivate : CineCameraWhenInFront.centerOnActivate;

                // Target tracking
                ComposerToOverwrite.TargetOffset = Vector3.Lerp(CineCameraWhenBehind.targetOffset, CineCameraWhenInFront.targetOffset, lerpAmount);
                OrbitalToOverwrite.TargetOffset = Vector3.Lerp(CineCameraWhenBehind.targetOffset, CineCameraWhenInFront.targetOffset, lerpAmount);
                OrbitalToOverwrite.TrackerSettings.PositionDamping = Vector3.Lerp(CineCameraWhenBehind.damping, CineCameraWhenInFront.damping, lerpAmount);

                // Lookahead
                ComposerToOverwrite.Lookahead.Enabled = lerpAmount < 0.5f ? CineCameraWhenBehind.lookahead : CineCameraWhenInFront.lookahead;
                ComposerToOverwrite.Lookahead.Time = Mathf.Lerp(CineCameraWhenBehind.time, CineCameraWhenInFront.time, lerpAmount);
                ComposerToOverwrite.Lookahead.Smoothing = Mathf.Lerp(CineCameraWhenBehind.smoothing, CineCameraWhenInFront.smoothing, lerpAmount);
                ComposerToOverwrite.Lookahead.IgnoreY = lerpAmount < 0.5f ? CineCameraWhenBehind.ignoreY : CineCameraWhenInFront.ignoreY;
        }

        #endregion

        [Space]
        public Transform Target;

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

        private bool _isCameraInFrontOfCharacter;

        private Vector3 _moveVelocity = Vector3.zero;

        private Vector3 _cashedTargetPosition;

        private Vector2 _joystickInputValues;
        private Vector2 _mouseInputValues;

        private Vector2 _rot;

        #endregion Util

        public void OnEnter ( CamBrain _brain ) {
                Brain = _brain;
                _position = _brain.CashedTransform.Position;
                _rotation = _brain.CashedTransform.Rotation;
               // _cashedTargetPosition = Target.GetComponentInChildren<Rigidbody>().position;
                _cashedTargetPosition = Target.position;

                CompareCameraDirectionToCharacter(true);
        }

        public void Execute ( float _delta ) {

                CompareCameraDirectionToCharacter();

                if (Target != null)
                {
                        _cashedTargetPosition = Target.position;
                }
                if (Brain.Input != null)
                {
                        InputHandling(_delta);
                }


                //_position = SmoothMove(Brain, UpdatePosition(_delta), _delta);
                _position = UpdatePosition(_delta);
                _rotation = UpdateRotation(_delta);
        }

        //Check if character is facing towards the camera and adjust the composer data accordingly.
        private void CompareCameraDirectionToCharacter (bool overwrite = false) {
                if(!ComposerToOverwrite) { return; }


                bool cameraCurrentlyInfrontOfCharacter = Vector3.Dot(Target.forward, ComposerToOverwrite.transform.forward) < 0;
                bool CurrentlyNotSet = _LerpBehindToInFront != 1 && _LerpBehindToInFront != 0;

                Debug.Log("Dot is " + Vector3.Dot(Target.forward, ComposerToOverwrite.transform.forward));
                Debug.Log("Lerp is " + _LerpBehindToInFront);
                if (CurrentlyNotSet  || cameraCurrentlyInfrontOfCharacter != _isCameraInFrontOfCharacter || overwrite)
                {
                        //if first time, set immediately to behind or in front
                        if (overwrite) _LerpBehindToInFront = cameraCurrentlyInfrontOfCharacter ? 1 : 0;
                        //if not, then move the lerp amount towards the goal. The ensures smooth changing between settings, not instant.
                        else
                        {
                                _LerpBehindToInFront = Mathf.MoveTowards(_LerpBehindToInFront, cameraCurrentlyInfrontOfCharacter ? 1 : 0, (1/0.13f) * Time.deltaTime);
                        }

                        ApplyComposerDataToComposer(_LerpBehindToInFront);
                        _isCameraInFrontOfCharacter = cameraCurrentlyInfrontOfCharacter;
                }
        }

        public void OnExit () {
                Brain = null;
        }

        #region AdditionalFunctions

        private void InputHandling ( float _delta ) {
                // looking behind where the player is facing
                if (Brain.Input.BackCameraInput.IsPressed())
                {
                        _rot.y = Mathf.LerpAngle(_rot.y, Target.eulerAngles.y + 180f, BackCameraSpeed * _delta);
                }
                else
                {
                        if ((_joystickInputValues + _mouseInputValues).magnitude < 0.1f)
                        {
                                _recenteringState -= _delta;
                                if (_recenteringState <= 0)
                                {
                                        _rot.x = Mathf.LerpAngle(_rot.x, Target.eulerAngles.x, YAxisRecenteringSpeed * _delta);
                                        _rot.y = Mathf.LerpAngle(_rot.y, Target.eulerAngles.y, XAxisRecenteringSpeed * _delta);
                                }

                        }
                        else
                        {
                                _recenteringState = CameraRecenteringWait;
                        }

                        // Screw joystick simulation we're going full SRB2
                        //_inputValues = Vector2.ClampMagnitude(Brain.Input.CameraInput.ReadValue<Vector2>(), 1);
                        _joystickInputValues = Brain.Input.CameraInputValues;
                        _mouseInputValues = Vector2.Lerp(_mouseInputValues, Brain.Input.MouseInput.ReadValue<Vector2>(), SmoothRotationSpeed * _delta);

                        // Idk why x and y values are swapped but i dont wanna fix it
                        _rot.y += _joystickInputValues.x * JoystickSensitivity.x * _delta;
                        _rot.x -= _joystickInputValues.y * JoystickSensitivity.y * _delta;
                        _rot.y += _mouseInputValues.x * MouseSensitivity.x * Time.timeScale; // Multiply by timescale so the camera wont rotate while paused
                        _rot.x -= _mouseInputValues.y * MouseSensitivity.y * Time.timeScale;

                        _rot.x = Mathf.Clamp(_rot.x, YLimits.x, YLimits.y);
                }
        }

        public Quaternion UpdateRotation ( float _delta ) {
                // Dunno why its here but it messes with using the mouse for rotation so it goes in the trash
                //return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(Target.position - _position), SmoothRotationSpeed * _delta);
                return Quaternion.LookRotation(Target.position - _position);
        }

        public Vector3 UpdatePosition ( float _delta ) {
                Quaternion _posRot = new()
                {
                        eulerAngles = new Vector3(_rot.x, _rot.y)
                };
                return _cashedTargetPosition + Offset + (_posRot * (Vector3.forward * TargetDistance));
        }

        public Vector3 SmoothMove ( CamBrain camBrain, Vector3 Position, float _delta ) {
                return Vector3.SmoothDamp(camBrain.CashedTransform.Position, Position, ref _moveVelocity, MovementSmoothing, Mathf.Infinity, _delta);
        }

        #endregion AdditionalFunctions

        private Vector3 _position;
        private Quaternion _rotation = Quaternion.identity;

        public PosRot Transform () {
                PosRot _transfrm = new()
                {
                        Position = _position,
                        Rotation = _rotation
                };
                return _transfrm;
        }
}