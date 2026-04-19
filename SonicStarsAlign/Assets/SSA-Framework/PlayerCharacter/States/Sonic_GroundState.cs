using System;
using TMPro;
using UnityEngine;

public class Sonic_GroundState : IState
{
        private readonly Sonic_PlayerStateMachine _ctx;
        private bool _groundDetected;
        private float _slipState;
        private bool _wereInWater;
        public Sonic_GroundState ( Sonic_PlayerStateMachine _machine ) {
                _ctx = _machine;
        }

        public void EnterState () {
                #region Misc

                _groundDetected = true;
                _wereInWater = _ctx.InWater;
                _ctx.AirDashes = 1;

                #endregion Misc

                #region Collision

                if (_ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal))
                {
                        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;

                        Vector3 targetPos =_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover;

                        Player_StaticFunctions.MoveRBPosition(_ctx.Rb, Vector3.Lerp(_ctx.Rb.position, targetPos, 0.5f), "Enter Ground State");
                }

                _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
                _ctx.Anim.SetInteger("State", 0);

                #endregion Collision

                #region Velocity

                _ctx.VerticalVelocity = Vector3.zero;
                _ctx.HorizontalVelocity =
                      Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal);

                _ctx.Physics_ApplyVelocity();

                InputRotations();

                #endregion Velocity
        }

        public void UpdateState () {
                //float _delta = Time.deltaTime;
        }

        public void FixedUpdateState () {
                float _delta = Time.fixedDeltaTime;

                if (_ctx.InWater && !_wereInWater && _ctx.CanRunOnWater())
                {
                        // Were on ground but entered water with enough speed - 
                        // Entering water run
                        _ctx.runningOnWater = true;
                        _ctx.InWater = false;
                        Debug.Log("Water run!");
                }

                // Skip normal groundcheck when running on water
                if (_ctx.runningOnWater)
                {
                        if (!_ctx.CanRunOnWater())
                        {
                                // Not fast enough - sink into water and fall
                                _ctx.runningOnWater = false;
                                _ctx.InWater = true;
                                AirSwitchConditions();
                                return;
                        }
                        if(!WaterCheck() && GroundCheck())
                        {
                                // Emerging on land
                                Debug.Log("Landing!");
                                _ctx.runningOnWater = false;
                                _ctx.InWater = false; 
                        }
                }
                else if (!GroundCheck())
                {
                        AirSwitchConditions();
                        return;
                }

                GroundApplication(_delta);
                SlopePhysics(_delta);
                if (SlipCheck())
                {
                        Slipment(_delta);
                        if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.SlopeAngle)
                        {
                                _groundDetected = false;
                                AirSwitchConditions();
                        }
                }

                Movement(_delta);

                GroundSwitchConditions();
                _ctx.Physics_ApplyVelocity();

                _ctx.RingCheck();
        }

        public void LateUpdateState () {
        }

        public void ExitState () {
                _ctx.runningOnWater = false;

                _ctx.CurrentMoveDirection = Vector3.zero;
                _ctx.PreviousMoveDirection = Vector3.zero;
        }

        #region Util

        private bool GroundCheck () {
                _groundDetected = _ctx.GroundCast.Execute(_ctx.transform.position, -_ctx.GroundNormal);
                return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, _ctx.GroundNormal) <= _ctx.Chp.MaxGroundDeviation;
        }

        private bool WaterCheck()
        {
                // Water acts as ground when running on water
                _groundDetected = _ctx.WaterCast.Execute(_ctx.transform.position, -_ctx.GroundNormal);
                if(_groundDetected) _ctx.GroundCast.HitInfo = _ctx.WaterCast.HitInfo; // Sometimes by genius is almost fightening
                return _groundDetected; 
        }

        private bool SlipCheck () {
                if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.MinGroundStickSpeed && Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) > FrameworkUtility.ESlipAngle)
                {
                        _slipState = _ctx.Chp.SlipTime;
                }
                if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) <= FrameworkUtility.FloorAngle)
                {
                        _slipState = 0;
                }
                return _slipState > 0;
        }

        private void GroundApplication ( float _delta ) {
                _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
                _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized * _ctx.Velocity.magnitude;

                Vector3 targetPos = _ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover;
                Vector3 point0 = targetPos;
                Vector3 point1 = targetPos;
                point0 -= _ctx.GroundNormal * 0.45f;
                point1 += _ctx.GroundNormal * 0.45f;
                Collider[] colliders = new Collider[5];

                Debug.DrawLine(point0, point1, Color.magenta);

                // Simple point check isn't enough, player can sometimes clip
                if (Physics.OverlapCapsuleNonAlloc(point0, point1, 0.25f, colliders, _ctx.wallLayer) < 2f) _ctx.Physics_Snap(targetPos);

                // If stopped => unlock movement
                if (_ctx.Velocity.magnitude < 0.1f)
                {
                        _ctx.MovementLockDistance = 0;
                }

                // Locking movement after a dash panel if needed
                if (_ctx.MovementLockDistance > 0 && Vector3.Distance(_ctx.transform.position, _ctx.MovementLockStartPos) < _ctx.MovementLockDistance)
                {
                        _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.PlayerDirection, _ctx.GroundNormal).normalized;
                        _ctx.InputVector = _ctx.PlayerDirection;
                }
                else
                {
                        InputRotations();
                        _ctx.MovementLockDistance = -1;
                }


                GroundRotation(_delta);
        }

        private void GroundRotation ( float _delta ) {
                // Smoothing looks bad in a loop, disabling it there
                if (Vector3.Distance(_ctx.transform.position, _ctx.MovementLockStartPos) < _ctx.MovementLockDistance)
                {
                        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundCast.HitInfo.normal);
                        return;
                }

                float _turnStrength = _ctx.ChrTurn.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
                if (_ctx.InputVector.magnitude >= 0.1 && !_ctx.Skid)
                {
                        _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.InputVector, _turnStrength, 0);
                }
                else if (_ctx.HorizontalVelocity.magnitude >= 0.1 && Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.PlayerDirection) > _ctx.Chp.TurnDeviationCap)
                {
                        _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.HorizontalVelocity.normalized, _turnStrength, 0);
                }

                //Also smoothen rotation
                Debug.Log(_ctx.transform.up + " " + _ctx.GroundCast.HitInfo.normal);
                _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, Vector3.Lerp(_ctx.transform.up, _ctx.GroundCast.HitInfo.normal, _delta * _ctx.Chp.RotationSmoothingSpeed));
        }

        private void Movement ( float _delta ) {
                _ctx.Skid = false;
                if (_ctx.InputVector.magnitude > 0.1)
                {
                        if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.BaseSpeed)
                        {
                                float _acceleration = _ctx.Chp.Acceleration * _delta;
                                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chp.BaseSpeed);
                        }
                        else
                        {
                                float _Drag = _ctx.Chp.GroundDrag * _delta;
                                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, _ctx.HorizontalVelocity.normalized * _ctx.Chp.BaseSpeed, _Drag);
                        }

                        if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
                        {
                                float _turnDeceleration = _ctx.Chp.TurnDeceleration.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
                        }

                        float _turnStrength = _ctx.Chp.TurnStrengthCurve.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
                        _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.PlayerDirection * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
                }
                else
                {
                        float _deceleration = _ctx.Chp.Deceleration * _delta;
                        _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _deceleration);
                }
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
        }

        private void Slipment ( float _delta ) {
                _ctx.Skid = false;

                _slipState -= _delta;
                _ctx.HorizontalVelocity += _ctx.Chp.SlopeFactor * _delta * Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal);
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
        }

        private void SlopePhysics ( float _delta ) {
                //if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) <= FrameworkUtility.FloorAngle) return;

                float _slopeFactor = _ctx.Chp.SlopeFactor * _delta;
                if (Vector3.Dot(_ctx.Gravity, _ctx.HorizontalVelocity.normalized) >= 0)
                {
                        _slopeFactor = _ctx.Chp.SlopeFactorDown * _delta;
                }
                _ctx.HorizontalVelocity += Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal) * _slopeFactor;
        }

        private void InputRotations () {
                _ctx.PreviousMoveDirection = _ctx.CurrentMoveDirection;

                _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
                    ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
                    : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);
                //_ctx.InputRotation = Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation;

                Vector3 cameraForward = _ctx.InputRef.forward;

                float lookBackFactor = _ctx.Input.BackCameraInput.IsPressed() ? -1f : 1f;
                Vector3 targetForward = cameraForward * lookBackFactor;

                _ctx.CurrentMoveDirection = Vector3.Lerp(_ctx.CurrentMoveDirection, targetForward, Time.deltaTime * _ctx.Chp.LookBackTransitionSpeed);
                _ctx.CurrentMoveDirection = Vector3.ProjectOnPlane(_ctx.CurrentMoveDirection, _ctx.GroundNormal).normalized;

                Quaternion adjustedRotation = Quaternion.LookRotation(_ctx.CurrentMoveDirection, _ctx.GroundNormal);
                _ctx.InputVector = adjustedRotation * _ctx.Input.VectorMoveInput.normalized;
        }

        private void GroundSwitchConditions () {
                if (_ctx.RingDetector.TargetDetected && _ctx.Input.ReactionInput.WasPressedThisFrame())
                {
                        _ctx.MachineTransition(PlayerStates.LightSpeedDash);
                }

                if (_ctx.Input.BounceInput.WasPressedThisFrame())
                {
                        _ctx.MachineTransition(PlayerStates.Spindash);
                        return;
                }

                if (_ctx.Input.CrouchInput.WasPressedThisFrame() && _ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinRollSpeed)
                {
                        _ctx.MachineTransition(PlayerStates.Roll);
                        _ctx.Snd.PlaySound("Roll");
                }

                if (_ctx.Input.JumpInput.WasPressedThisFrame())
                {
                        _ctx.Anim.SetInteger("State", 1);
                        _ctx.Jump();
                }
                if (_ctx.Input.SweepInput.WasPressedThisFrame() && _ctx.HorizontalVelocity.magnitude > _ctx.Chp.RunSpeedThreshold)
                {
                        _ctx.MachineTransition(PlayerStates.SweepKick);
                        return;
                }
        }

        private void AirSwitchConditions () {
                _ctx.MachineTransition(PlayerStates.Air);
        }

        #endregion Util
}