using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;

// Camera movement
public class CamPoint_NormalPlayer : MonoBehaviour, ICamPointStyle
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
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f), DrawTickBoxBefore("UseTargetTurnOffset")]public Transform TargetTurnOffset;
        [HideInInspector] public bool UseTargetTurnOffset;


        #region Cinemachine Data

        [Header("Cinemachine Camera Control")]
        [TextSpace("The below composers will overwrite the provided Cinemachine Position Composer, depending if the camera is behind or infront of the character.")]
        public bool DummyText2;

        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineCamera CMCamera;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineRotationComposer ComposerToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineOrbitalFollow OrbitalToOverwrite;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)]       public CinemachineDeoccluder DeoccluderToOverwrite;


        private float _LerpBehindToInFront;
        private Cinemachine3OrbitRig.Settings _CurrentOrbitSize = new Cinemachine3OrbitRig.Settings();

        #endregion

        [Header("Objects With Data")]
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)] public CameraStatsPrimary PrimaryStats;
        [ColourIfNull(0.6f , 0.2f , 0.2f , 2f)] public CameraStatsEffects EffectStats;

        #region Util

        private float _currentPlayerRunningSpeed;
        private float _previousPlayerRunningSpeed;

        private float _recenteringState;
        private bool _canCheckSpeedForRecenter = true;

        private bool _isCameraInFrontOfCharacter;
        private bool _canSetIsCameraInFrontOfCharacter = true;
        private float _CharacterCameraDot;

        private Vector2 _rot;

        //Tracking stats of camera
        private float _currentFOV = 70;
        private float _currentDistanceModifier = 1;

        private Vector2 _previousCameraAxisValues;
        private Vector2 _currentCameraAxisValues;
        private float _amountMovedHorizThisFrame;
        private Vector2 _inputThisFrame;

        //Offsets
        private float _currentLookAheadModifier;
        private float _currentTurnOffset = 0;
        private float _timeTurningPlayer;

        private Vector3 _previousVerticalOffsetPosition;

        #endregion Util

        public void OnEnterPoint ( CamBrain _brain ) {
                Brain = _brain;

                //Cinemachine setup
                CMCamera.Target.TrackingTarget = MainTarget;
                DeoccluderToOverwrite.CollideAgainst = EffectStats.CameraCollidesWith;
                ComposerToOverwrite.enabled = true;
                OrbitalToOverwrite.enabled = true;

                CompareCameraDirectionToCharacter(true);
        }

        public void ExecutePoint ( float _delta ) {

                if(Pause_Manager.paused) { return; }

                GetCurrentCameraState();

                _currentPlayerRunningSpeed = PlayerCTX.PlayerRunningSpeed;

                CompareCameraDirectionToCharacter();
                CalculateTargetPlacement();
                CalculateFOV();

                AutoRecenterCamera(false);

                _previousPlayerRunningSpeed = _currentPlayerRunningSpeed;

                SetPreviousCameraState();
        }


        public void OnExitPoint () {
                ComposerToOverwrite.enabled = false;
                OrbitalToOverwrite.enabled = false;
                //Brain = null;
        }

        #region camera calculations

        //Central location for getting all values that may be relevant this frame.
        private void GetCurrentCameraState () {
                _currentCameraAxisValues = new Vector2(OrbitalToOverwrite.HorizontalAxis.Value, OrbitalToOverwrite.VerticalAxis.Value);
                _amountMovedHorizThisFrame = Mathf.Abs(_currentCameraAxisValues.x - _previousCameraAxisValues.x);
        }

        //After all other code, set values to be compared to next frame.
        private void SetPreviousCameraState () {
                _previousCameraAxisValues = _currentCameraAxisValues;
        }

        //Check if character is facing towards the camera and adjust the CM data accordingly.
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

                        //ApplyComposerDataToComposer(_LerpBehindToInFront);
                        PrimaryStats.ApplyComposerDataToComposer(ref _CurrentOrbitSize, ref OrbitalToOverwrite, ref ComposerToOverwrite, ref _currentLookAheadModifier, _LerpBehindToInFront);
                        SetIsCameraInFrontOfCharacter(cameraCurrentlyInfrontOfCharacter);
                }
        }
        private bool IsPlayerAccelerating () {
                return (_currentPlayerRunningSpeed >= _previousPlayerRunningSpeed) && _currentPlayerRunningSpeed > 1;
        }

        private void CalculateFOV () {
                float targetFOV = EffectStats.FOVBySpeed.Evaluate(_currentPlayerRunningSpeed);
                bool increaseValue = IsPlayerAccelerating();

                //Either lerp or move values directly
                switch (EffectStats.HowToAdjustFOV)
                {
                        case CameraStatsEffects.TypeOfFOVLerp.lerp:
                                float minAmount = (increaseValue ? EffectStats.minAmountToAdjustFOV.x : EffectStats.minAmountToAdjustFOV.y) * Time.deltaTime;
                                float newFOV = Mathf.Lerp(_currentFOV, targetFOV, (increaseValue ? EffectStats.lerpSpeeds.x : EffectStats.lerpSpeeds.y));

                                if (Mathf.Abs(newFOV - _currentFOV) < minAmount)
                                        newFOV = Mathf.MoveTowards(_currentFOV, targetFOV, minAmount);
                                _currentFOV = newFOV;

                                break;
                        case CameraStatsEffects.TypeOfFOVLerp.moveTowards:
                                _currentFOV = Mathf.MoveTowards(_currentFOV, targetFOV, Time.deltaTime * (increaseValue ? EffectStats.moveTowardsFOVSpeeds.x : EffectStats.moveTowardsFOVSpeeds.y));
                                break;
                }
                CMCamera.Lens.FieldOfView = _currentFOV;

                //Apply distance relative to modifier, to ensure player doesn't become too small.
                _currentDistanceModifier = EffectStats.DistanceModifierByFOV.Evaluate(_currentFOV);
                OrbitalToOverwrite.Orbits.Top.Radius = _CurrentOrbitSize.Top.Radius * _currentDistanceModifier;
                OrbitalToOverwrite.Orbits.Center.Radius = _CurrentOrbitSize.Center.Radius * _currentDistanceModifier;
                OrbitalToOverwrite.Orbits.Bottom.Radius = _CurrentOrbitSize.Bottom.Radius * _currentDistanceModifier;
        }

        //Calculates a number of offsets for the main target, then places it in the middle of them.
        private void CalculateTargetPlacement () {
                Vector3 BaseTargetPosition = BaseTarget.position;
                MainTarget.position = BaseTargetPosition;

                Vector3 TargetOffset = Vector3.zero;

                OffsetByLookAhead();
                OffsetByVertical();
                OffsetByTurn();
                OffsetIfWallRun();

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

                        TargetOffset += TargetLookAheadOffset.position - BaseTargetPosition;
                }

                //Takes how much the camera is look from above or below the character, and moves the target slightly up or down. This allows the camera to look up without being stuck under the character model.
                void OffsetByVertical () {
                        if (!UseTargetVerticalOffset || !TargetAdditionalVerticalOffset)
                                return;

                        float angle = Vector3.Angle(PlayerRB.transform.up, ComposerToOverwrite.transform.forward);

                        Vector3 newPosition = PlayerRB.transform.up * EffectStats.VerticalOffsetByViewAngle.Evaluate(angle);
                        TargetOffset += Vector3.Lerp(_previousVerticalOffsetPosition, newPosition, 0.2f);
                        TargetAdditionalVerticalOffset.position = BaseTargetPosition + Vector3.Lerp(_previousVerticalOffsetPosition, newPosition, 0.2f);

                        _previousVerticalOffsetPosition = newPosition;
                }

                //Takes how much the camera is looking at the side of the player, and how long the player has been turning for, then offsets camera to its left or right.
                void OffsetByTurn () {
                        if (!UseTargetTurnOffset || !TargetTurnOffset)
                                return;

                        if (_currentPlayerRunningSpeed < 10 && _currentTurnOffset < 0.01f)
                                return;

                        //Calculate angle without up and down, to compare to player.
                        Vector3 relativeAngle = PlayerRB.transform.InverseTransformDirection(ComposerToOverwrite.transform.forward);
                        relativeAngle.y = 0;
                        relativeAngle = PlayerRB.transform.TransformDirection(relativeAngle);
                        float angleDifference = Vector3.Angle(PlayerRB.linearVelocity.normalized, relativeAngle);

                        //Detects left or right, so knows if player changes direction.
                        bool turningRight = Vector3.Dot(PlayerRB.transform.right, relativeAngle) < 0;
                        if (_isCameraInFrontOfCharacter) turningRight = !turningRight;

                        //If player is turning, start moving to offset.
                        if (_currentPlayerRunningSpeed > 9 && angleDifference > 8 && angleDifference < 170)
                        {
                                _timeTurningPlayer = Mathf.MoveTowards(_timeTurningPlayer, 3, Time.deltaTime); //Offset max will gradually ramp up for long turns.
                                _currentTurnOffset = Mathf.Lerp(_currentTurnOffset, 
                                        EffectStats.TurnOffsetByAngle.Evaluate(angleDifference) * EffectStats.TurnOffsetMultiplyByTime.Evaluate(_timeTurningPlayer)* (turningRight ? 1f : -1f), 
                                        EffectStats.TurnOffsetLerpSpeed.x * Time.deltaTime);
                        }
                        //If not turning, gradually remove offset.
                        else
                        {
                                _timeTurningPlayer = Mathf.MoveTowards(_timeTurningPlayer, 0, Time.deltaTime * 3);
                                _currentTurnOffset = Mathf.Lerp(_currentTurnOffset, 0, EffectStats.TurnOffsetLerpSpeed.y * Time.deltaTime);
                        }

                        //Offset could be annoying if turning camera around player, so lessen offset if this happens.
                        if(_amountMovedHorizThisFrame > 2.5f)
                        {
                                _timeTurningPlayer = Mathf.MoveTowards(_timeTurningPlayer, 0, Time.deltaTime * 2f);
                                _currentTurnOffset = Mathf.Lerp(_currentTurnOffset, 0, EffectStats.TurnOffsetLerpSpeed.y * Time.deltaTime);
                        }
                     
                        //Setting
                        Vector3 offSetDirection = ComposerToOverwrite.transform.right;          
                        offSetDirection = Vector3.ProjectOnPlane(offSetDirection.normalized, PlayerRB.transform.up);

                        TargetTurnOffset.position = BaseTargetPosition + offSetDirection * _currentTurnOffset;
                        TargetOffset += offSetDirection * _currentTurnOffset;
                }

                // Offset the camera from the wall when wall running
                void OffsetIfWallRun()
                {
                        if(PlayerCTX.CurrentEstate == PlayerStates.WallRun)
                        {
                                TargetOffset += PlayerCTX.WallRunNormal * EffectStats.WallRunOffset;
                        }
                }

        }

        #endregion

        #region effects

        #endregion

        #region camera control

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
                        if (PlayerRB.linearVelocity.sqrMagnitude > EffectStats.MinSpeedToAutoRecenter.x * EffectStats.MinSpeedToAutoRecenter.x)
                        {
                                OrbitalToOverwrite.HorizontalAxis.Recentering.Enabled = true;
                        }
                        else if (PlayerRB.linearVelocity.sqrMagnitude * 0.8f < EffectStats.MinSpeedToAutoRecenter.x * EffectStats.MinSpeedToAutoRecenter.x)
                                OrbitalToOverwrite.HorizontalAxis.Recentering.Enabled = false;
                }

                //Vertical Recentering
                if (PlayerRB.linearVelocity.sqrMagnitude > EffectStats.MinSpeedToAutoRecenter.y * EffectStats.MinSpeedToAutoRecenter.y)
                {
                        OrbitalToOverwrite.VerticalAxis.Recentering.Enabled = true;
                }
                else if (PlayerRB.linearVelocity.sqrMagnitude * 0.8f < EffectStats.MinSpeedToAutoRecenter.y * EffectStats.MinSpeedToAutoRecenter.y)
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
                if(!_canSetIsCameraInFrontOfCharacter) { return; }

                if (value != _isCameraInFrontOfCharacter)
                {
                        //If now in front of character
                        if (value)
                        {
   
                                StartCoroutine(TempDisableCheckSpeedForRecenter(0.5f));
                        }
                        //If now behind character
                        else
                        {

                        }

                        _isCameraInFrontOfCharacter = value;
                        StartCoroutine(DelayDetectingIfCameraInFront());
                }
        }

        //To prevent weird cases where InFront is immediately set back to the inverse, add a delay.
        private IEnumerator DelayDetectingIfCameraInFront () {
                _canSetIsCameraInFrontOfCharacter = false;
                yield return new WaitForSecondsRealtime(0.3f);
                _canSetIsCameraInFrontOfCharacter = true;
        }

        #endregion AdditionalFunctions
}