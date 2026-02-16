using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Camera movement
public class CamPoint_NormalPlayer : MonoBehaviour, ICamPoint
{

        [SerializeField,TextSpace("This component handles all default behaviour for the camera when it is locked on the character in normal gameplay. Control, effects, follow, etc.")]
        bool DummyText;

        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]public CamBrain Brain;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]public Rigidbody PlayerRB;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]public Sonic_PlayerStateMachine PlayerCTX;

        [Header("Camera Target and Subtargets")]
        [SerializeField,TextSpace("The target may need to be moved at certain points, such as to avoid being too close to a wall. These different objects are used to calculate the position of the Main Target")]
        bool DummyText3;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)][Tooltip("This is the actual target being looked at, but its placement is affected by the others")] public Transform MainTarget;
        [ColourIfNull(0.6f, 0.2f, 0.2f, 2f)][Tooltip("Must be a parent of the other targets, and a child of the character. When in doubt, use this.")]public Transform BaseTarget;
        [ColourIfNull(0.6f, 0.2f, 0.2f, 2f), DrawTickBoxBefore("UseTargetLookAheadOffset")]public Transform TargetLookAheadOffset;
        [HideInInspector] public bool UseTargetLookAheadOffset;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f), DrawTickBoxBefore("UseTargetVerticalOffset")]public Transform TargetAdditionalVerticalOffset;
        [HideInInspector] public bool UseTargetVerticalOffset;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f), DrawTickBoxBefore("UseTargetShoulderOffset")]public Transform TargetShoulderOffset;
        [HideInInspector] public bool UseTargetShoulderOffset;


        #region Cinemachine Data

        [Header("Cinemachine Camera Control")]
        [TextSpace("The below composers will overwrite the provided Cinemachine Position Composer, depending if the camera is behind or infront of the character.")]
        public bool DummyText2;

        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineCamera CMCamera;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineRotationComposer ComposerToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineOrbitalFollow OrbitalToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineDeoccluder DeoccluderToOverwrite;

        public CineCameraData CineCameraWhenBehind;
        public CineCameraData CineCameraWhenInFront;

        private float _LerpBehindToInFront;
        private Cinemachine3OrbitRig.Settings _CurrentOrbitSize = new Cinemachine3OrbitRig.Settings();

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
                [Range(0,3)]
                public float lookAheadModifier;
   
        }

        public void ApplyComposerDataToComposer ( float lerpAmount ) {
                // Framing / distance
                _CurrentOrbitSize.Top.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Top.Radius, CineCameraWhenInFront.OrbitNewSettings.Top.Radius, lerpAmount);
                OrbitalToOverwrite.Orbits.Top.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Top.Height, CineCameraWhenInFront.OrbitNewSettings.Top.Height, lerpAmount);
                _CurrentOrbitSize.Center.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Center.Radius, CineCameraWhenInFront.OrbitNewSettings.Center.Radius, lerpAmount);
                OrbitalToOverwrite.Orbits.Center.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Center.Height, CineCameraWhenInFront.OrbitNewSettings.Center.Height, lerpAmount);
                _CurrentOrbitSize.Bottom.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Bottom.Radius, CineCameraWhenInFront.OrbitNewSettings.Bottom.Radius, lerpAmount);
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
                _currentLookAheadModifier = Mathf.Lerp(CineCameraWhenBehind.lookAheadModifier, CineCameraWhenInFront.lookAheadModifier, lerpAmount);
        }

        #endregion

        [Header("Parameters")]
        public LayerMask CameraCollidesWith;
        public float BaseDistanceModifier = 1;

        [Header("Tracking and view")]
        public AnimationCurve FOVBySpeed = new AnimationCurve( new Keyframe[] { 
                new Keyframe(0, 70),
                new Keyframe(25, 90f),
        } );
        public AnimationCurve DistanceModifierBySpeed = new AnimationCurve( new Keyframe[] { 
                new Keyframe(0, 1),
                new Keyframe(25, 0.5f),
        } );
        [Tooltip("The x is the angle difference between camera and characters current 'up' vector, 0 - 180. Y is the vertical offset of the target up or down.")]
        public AnimationCurve VerticalOffsetByViewAngle = new AnimationCurve( new Keyframe[] {
                new Keyframe(0, 0.3f),
                new Keyframe(90, 0),
                new Keyframe(180, 0.3f),
        } );

        [Header("Recentering")]
        public Vector2 MinSpeedToAutoRecenter;

        [Header("Legacy Parameters")]
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

        private float _currentPlayerRunningSpeed;

        private float _recenteringState;
        private bool _canCheckSpeedForRecenter = true;

        private bool _isCameraInFrontOfCharacter;
        private float _CharacterCameraDot;

        private Vector3 _cashedTargetPosition;

        private Vector2 _joystickInputValues;
        private Vector2 _mouseInputValues;

        private Vector2 _rot;

        //Tracking stats of camera
        private float _currentLookAheadModifier;
        private float _currentFOV = 70;
        private float _currentDistanceModifier = 1;

        private Vector3 _previousVerticalOffsetPosition;

        #endregion Util

        public void OnEnter ( CamBrain _brain ) {
                Brain = _brain;
                _position = _brain.CashedTransform.Position;
                _rotation = _brain.CashedTransform.Rotation;
                // _cashedTargetPosition = Target.GetComponentInChildren<Rigidbody>().position;
                _cashedTargetPosition = MainTarget.position;

                //Cinemachine setup
                CMCamera.Target.TrackingTarget = MainTarget;
                DeoccluderToOverwrite.CollideAgainst = CameraCollidesWith;

                CompareCameraDirectionToCharacter(true);
        }

        public void Execute ( float _delta ) {

                _currentPlayerRunningSpeed = PlayerCTX.PlayerRunningSpeed;

                CompareCameraDirectionToCharacter();
                CalculateTargetPlacement();
                CalculateFOV();
                CalculateDistanceModifier();

                AutoRecenterCamera(false);

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

                _CharacterCameraDot = Vector3.Dot(MainTarget.forward, ComposerToOverwrite.transform.forward);
                bool cameraCurrentlyInfrontOfCharacter = _CharacterCameraDot < 0;
                bool CurrentlyNotSet = _LerpBehindToInFront != 1 && _LerpBehindToInFront != 0;

                if (CurrentlyNotSet || cameraCurrentlyInfrontOfCharacter != _isCameraInFrontOfCharacter || overwrite)
                {
                        //if first time, set immediately to behind or in front
                        if (overwrite) _LerpBehindToInFront = cameraCurrentlyInfrontOfCharacter ? 1 : 0;
                        //if not, then move the lerp amount towards the goal. The ensures smooth changing between settings, not instant.
                        else
                        {
                                float howLongLerpTakes = 0.8f;
                                _LerpBehindToInFront = Mathf.MoveTowards(_LerpBehindToInFront, cameraCurrentlyInfrontOfCharacter ? 1 : 0, (1 / howLongLerpTakes) * Time.deltaTime);
                        }

                        ApplyComposerDataToComposer(_LerpBehindToInFront);
                        SetIsCameraInFrontOfCharacter(cameraCurrentlyInfrontOfCharacter);
                }
        }

        private void CalculateFOV () {
                _currentFOV = Mathf.Lerp(_currentFOV, FOVBySpeed.Evaluate(_currentPlayerRunningSpeed), 0.2f);
                CMCamera.Lens.FieldOfView = _currentFOV;
        }

        private void CalculateDistanceModifier () {
                _currentDistanceModifier = Mathf.Lerp(_currentDistanceModifier, DistanceModifierBySpeed.Evaluate(_currentPlayerRunningSpeed), 0.2f);
                OrbitalToOverwrite.Orbits.Top.Radius = _CurrentOrbitSize.Top.Radius * _currentDistanceModifier;
                OrbitalToOverwrite.Orbits.Center.Radius = _CurrentOrbitSize.Center.Radius * _currentDistanceModifier;
                OrbitalToOverwrite.Orbits.Bottom.Radius = _CurrentOrbitSize.Bottom.Radius * _currentDistanceModifier;
        }

        //Calculates a number of offsets for the main target, then places it in the middle of them.
        private void CalculateTargetPlacement () {
                Vector3 BaseTargetPosition = BaseTarget.position;
                MainTarget.position = BaseTargetPosition;

                Vector3 TargetOffset = Vector3.zero;
                float totalOffsets = 0;

                OffsetByLookAhead();
                OffsetByVertical();

                //MainTarget.position = BaseTargetPosition + (TargetOffset / Mathf.Max(1, totalOffsets));
                MainTarget.position = BaseTargetPosition + TargetOffset;

                //When moving, target will be slightly offset in direction of velocity, focussing more on what is coming.
                void OffsetByLookAhead () {
                        if (!UseTargetLookAheadOffset || !TargetLookAheadOffset)
                        {
                                return;
                        }

                        if (PlayerRB.linearVelocity.sqrMagnitude > 1 * 1)
                        {
                                TargetLookAheadOffset.position = BaseTargetPosition + PlayerRB.linearVelocity * Time.fixedDeltaTime * _currentLookAheadModifier;
                        }
                        else
                                TargetLookAheadOffset.localPosition = Vector3.Lerp(TargetLookAheadOffset.localPosition, Vector3.zero, 0.2f);

                        if (PlayerRB.linearVelocity.sqrMagnitude > 10 * 10 || TargetLookAheadOffset.localPosition.sqrMagnitude > 0.1)
                        {
                                TargetOffset += TargetLookAheadOffset.position - BaseTargetPosition;
                                totalOffsets++;
                        }
                }

                //Takes how much the camera is look from above or below the character, and moves the target slightly up or down. This allows the camera to look up without being stuck under the character model.
                void OffsetByVertical () {
                        if (!UseTargetVerticalOffset || !TargetAdditionalVerticalOffset)
                                return;
      
                        float angle = Vector3.Angle(PlayerRB.transform.up, ComposerToOverwrite.transform.forward);

                        Vector3 newPosition = PlayerRB.transform.up * VerticalOffsetByViewAngle.Evaluate(angle);
                        TargetOffset += Vector3.Lerp(_previousVerticalOffsetPosition, newPosition, 0.2f);
                        TargetAdditionalVerticalOffset.position = BaseTargetPosition + Vector3.Lerp(_previousVerticalOffsetPosition, newPosition, 0.2f);
                        totalOffsets++;

                        _previousVerticalOffsetPosition = newPosition;
                }


        }

        #endregion

        #region effects

        #endregion

        #region camera control
        private void InputHandling ( float _delta ) {
                // looking behind where the player is facing
                if (Brain.Input.BackCameraInput.IsPressed())
                {
                        AutoRecenterCamera(true);
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

        private void AutoRecenterCamera ( bool ManualOverwrite ) {

                if (!_canCheckSpeedForRecenter)
                        return;

                if (ManualOverwrite)
                {
                        Debug.Log("Trigger recenter");
                        OrbitalToOverwrite.HorizontalAxis.TriggerRecentering();
                        OrbitalToOverwrite.VerticalAxis.TriggerRecentering();

                        StartCoroutine(TempDisableCheckSpeedForRecenter(Mathf.Max(OrbitalToOverwrite.VerticalAxis.Recentering.Time, OrbitalToOverwrite.HorizontalAxis.Recentering.Time), 0.6f));
                        OrbitalToOverwrite.VerticalAxis.Recentering.Time *= 0.6f;
                        OrbitalToOverwrite.HorizontalAxis.Recentering.Time *= 0.6f;
                        return;
                }

                if (!PlayerRB) { return; }

                //Horizontal Recentering
                if (_CharacterCameraDot < -0.9f)//if character is running towards camera, then dont rotate behind them, as that messes inputs.
                        OrbitalToOverwrite.HorizontalAxis.Recentering.Enabled = false;
                else
                {
                        if (PlayerRB.linearVelocity.sqrMagnitude > MinSpeedToAutoRecenter.x * MinSpeedToAutoRecenter.x)
                        {
                                OrbitalToOverwrite.HorizontalAxis.Recentering.Enabled = true;
                        }
                        else if (PlayerRB.linearVelocity.sqrMagnitude * 0.8f < MinSpeedToAutoRecenter.x * MinSpeedToAutoRecenter.x)
                                OrbitalToOverwrite.HorizontalAxis.Recentering.Enabled = false;
                }

                //Vertical Recentering
                if (PlayerRB.linearVelocity.sqrMagnitude > MinSpeedToAutoRecenter.y * MinSpeedToAutoRecenter.y)
                {
                        OrbitalToOverwrite.VerticalAxis.Recentering.Enabled = true;
                }
                else if (PlayerRB.linearVelocity.sqrMagnitude * 0.8f < MinSpeedToAutoRecenter.y * MinSpeedToAutoRecenter.y)
                        OrbitalToOverwrite.VerticalAxis.Recentering.Enabled = false;
        }

        private IEnumerator TempDisableCheckSpeedForRecenter ( float seconds, float modifierOnTime = 1 ) {
                _canCheckSpeedForRecenter = false;
                OrbitalToOverwrite.HorizontalAxis.Recentering.Enabled = false;
                OrbitalToOverwrite.VerticalAxis.Recentering.Enabled = false;

                yield return new WaitForSeconds(seconds * 1.2f);
                OrbitalToOverwrite.VerticalAxis.Recentering.Time /= modifierOnTime;
                OrbitalToOverwrite.HorizontalAxis.Recentering.Time /= modifierOnTime;

                _canCheckSpeedForRecenter = true;
        }
        #endregion

        #region AdditionalFunctions

        private void SetIsCameraInFrontOfCharacter ( bool value ) {
                if (value != _isCameraInFrontOfCharacter)
                {
                        //If now behind character
                        if (!value)
                        {

                        }
                        //If now in front of character
                        else
                        {
                                StartCoroutine(TempDisableCheckSpeedForRecenter(0.5f));
                        }

                        _isCameraInFrontOfCharacter = value;
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