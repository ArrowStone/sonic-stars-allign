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

        [Header("Camera Target Levels")]
        [SerializeField,TextSpace("The target may need to be moved at certain points, such as to avoid being too close to a wall. These different objects are used to calculate the position of the Main Target")]
        bool DummyText3;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)][Tooltip("This is the actual target being looked at, but its placement is affected by the others")] public Transform MainTarget;
        [ColourIfNull(0.6f, 0.2f, 0.2f, 2f)][Tooltip("Must be a parent of the other targets, and a child of the character. When in doubt, use this.")]public Transform BaseTarget;
        [ColourIfNull(0.6f, 0.2f, 0.2f, 2f)]public Transform TargetCollisionOffset;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]public Transform TargetAdditionalVerticalOffset;


        #region Cinemachine Data

        [Header("Cinemachine Camera Control")]
        [TextSpace("The below composers will overwrite the provided Cinemachine Position Composer, depending if the camera is behind or infront of the character.")]
        public bool DummyText2;

        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineCamera CMCamera;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineRotationComposer ComposerToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineOrbitalFollow OrbitalToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineDeoccluder DeoccluderToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineDecollider DecolliderToOverwrite;

        public CineCameraData CineCameraWhenBehind;
        public CineCameraData CineCameraWhenInFront;

        private float _LerpBehindToInFront;

        [System.Serializable]
        public class CineCameraData
        {
                public Vector2 screenPosition;
                public Cinemachine3OrbitRig.Settings OrbitNewSettings;

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
                OrbitalToOverwrite.Orbits.Top.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Top.Radius, CineCameraWhenInFront.OrbitNewSettings.Top.Radius, lerpAmount);
                OrbitalToOverwrite.Orbits.Top.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Top.Height, CineCameraWhenInFront.OrbitNewSettings.Top.Height, lerpAmount);
                OrbitalToOverwrite.Orbits.Center.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Center.Radius, CineCameraWhenInFront.OrbitNewSettings.Center.Radius, lerpAmount);
                OrbitalToOverwrite.Orbits.Center.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Center.Height, CineCameraWhenInFront.OrbitNewSettings.Center.Height, lerpAmount);
                OrbitalToOverwrite.Orbits.Bottom.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Bottom.Radius, CineCameraWhenInFront.OrbitNewSettings.Bottom.Radius, lerpAmount);
                OrbitalToOverwrite.Orbits.Bottom.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Bottom.Height, CineCameraWhenInFront.OrbitNewSettings.Bottom.Height, lerpAmount);

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

        [Header("Parameters")]
        public LayerMask CameraCollidesWith;
        public float TargetDistance;

        public Vector3 Offset;


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
                _cashedTargetPosition = MainTarget.position;

                //Cinemachine setup
                CMCamera.Target.TrackingTarget = MainTarget;
                DecolliderToOverwrite.Decollision.ObstacleLayers = CameraCollidesWith;
                DeoccluderToOverwrite.CollideAgainst = CameraCollidesWith;

                CompareCameraDirectionToCharacter(true);
        }

        public void Execute ( float _delta ) {

                CompareCameraDirectionToCharacter();
                CalculateTargetPlacement();

                if (MainTarget != null)
                {
                        _cashedTargetPosition = MainTarget.position;
                }
                if (Brain.Input != null)
                {
                        InputHandling(_delta);
                }


                //_position = SmoothMove(Brain, UpdatePosition(_delta), _delta);
                _position = UpdatePosition(_delta);
                _rotation = UpdateRotation(_delta);
        }


        public void OnExit () {
                Brain = null;
        }

        #region camera calculations

        //Check if character is facing towards the camera and adjust the composer data accordingly.
        private void CompareCameraDirectionToCharacter ( bool overwrite = false ) {
                if (!ComposerToOverwrite || !OrbitalToOverwrite) { return; }

                bool cameraCurrentlyInfrontOfCharacter = Vector3.Dot(MainTarget.forward, ComposerToOverwrite.transform.forward) < 0;
                bool CurrentlyNotSet = _LerpBehindToInFront != 1 && _LerpBehindToInFront != 0;

                if (CurrentlyNotSet || cameraCurrentlyInfrontOfCharacter != _isCameraInFrontOfCharacter || overwrite)
                {
                        //if first time, set immediately to behind or in front
                        if (overwrite) _LerpBehindToInFront = cameraCurrentlyInfrontOfCharacter ? 1 : 0;
                        //if not, then move the lerp amount towards the goal. The ensures smooth changing between settings, not instant.
                        else
                        {
                                _LerpBehindToInFront = Mathf.MoveTowards(_LerpBehindToInFront, cameraCurrentlyInfrontOfCharacter ? 1 : 0, (1 / 0.13f) * Time.deltaTime);
                        }

                        ApplyComposerDataToComposer(_LerpBehindToInFront);
                        _isCameraInFrontOfCharacter = cameraCurrentlyInfrontOfCharacter;
                }
        }

        //Calculates a number of offsets for the main target, then places it in the middle of them.
        private void CalculateTargetPlacement () {
                Vector3 BaseTargetPosition = BaseTarget.position;
                MainTarget.position = BaseTargetPosition;

                Vector3 TargetOffset = Vector3.zero;
                float totalOffsets = 0;

                OffsetByCollision();
                MainTarget.position = BaseTargetPosition + (TargetOffset / Mathf.Max(1, totalOffsets));

                void OffsetByCollision () {
                        //Searches for any collision nearby to the target, and is it finds it, moves the target slightly away from it to ensure the camera won't be forced into it.
                        if (TargetCollisionOffset)
                        {
                                //For efficiency, first used overlap sphere
                                float range = 0.5f;
                                Debug.DrawRay(BaseTargetPosition, Vector3.up * range, Color.magenta);
                                Collider[] hits = Physics.OverlapSphere(BaseTargetPosition, range,CameraCollidesWith);
                                if (hits != null && hits.Length != 0)
                                {
                                        // Boxcast is slower than overlap sphere, but can provide a contact point. Cast roughly towards collision object.
                                        Vector3 direction = S_S_MoreMaths.GetDirection(BaseTargetPosition ,hits[0].transform.position);
                                        Debug.DrawLine(hits[0].transform.position, BaseTargetPosition, Color.cyan, 2f);

                                        if (Physics.BoxCast(BaseTargetPosition, new Vector3(range / 3, range / 3, 0.02f), direction, out RaycastHit hit,
                                                Quaternion.LookRotation(hits[0].transform.position, BaseTarget.up), range - 0.02f, CameraCollidesWith))
                                        {
                                                if (Vector3.Distance(hit.point, BaseTargetPosition) > range) 
                                                { return; }

                                                Vector3 closestPoint = hit.point;
                                                Debug.DrawLine(hit.point, BaseTargetPosition, Color.cyan, 2f);

                                                //Offset is inversly proportionate to how close to the collision. If collision point is 25% of range close, offset with be 75% in the opposite direction.
                                                closestPoint -= BaseTargetPosition;
                                                Vector3 collisionOffset = Vector3.Lerp(closestPoint, -closestPoint.normalized * range, 0.5f);
                                                collisionOffset *= 2;

                                                Debug.Log(collisionOffset.magnitude + " + " + closestPoint.magnitude + "  =  " + (collisionOffset.magnitude + closestPoint.magnitude));
                                                TargetOffset += collisionOffset;
                                                TargetCollisionOffset.position = (-closestPoint.normalized * range) + BaseTargetPosition;
                                                totalOffsets++;
                                        }
                                }
                        }
                }
        }

        #endregion


        #region AdditionalFunctions

        private void InputHandling ( float _delta ) {
                // looking behind where the player is facing
                if (Brain.Input.BackCameraInput.IsPressed())
                {
                        _rot.y = Mathf.LerpAngle(_rot.y, MainTarget.eulerAngles.y + 180f, BackCameraSpeed * _delta);
                }
                else
                {
                        if ((_joystickInputValues + _mouseInputValues).magnitude < 0.1f)
                        {
                                _recenteringState -= _delta;
                                if (_recenteringState <= 0)
                                {
                                        _rot.x = Mathf.LerpAngle(_rot.x, MainTarget.eulerAngles.x, YAxisRecenteringSpeed * _delta);
                                        _rot.y = Mathf.LerpAngle(_rot.y, MainTarget.eulerAngles.y, XAxisRecenteringSpeed * _delta);
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
                return Quaternion.LookRotation(MainTarget.position - _position);
        }

        public Vector3 UpdatePosition ( float _delta ) {
                Quaternion _posRot = new()
                {
                        eulerAngles = new Vector3(_rot.x, _rot.y)
                };
                return _cashedTargetPosition + Offset + (_posRot * (Vector3.forward * TargetDistance));
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