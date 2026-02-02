using UnityEngine;
using UnityEngine.EventSystems;

public class Sonic_AirState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _ddchargeTime;
    private float _airDragTime;

    public Sonic_AirState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _groundDetected = false;
        _ddchargeTime = 0;
        _airDragTime = 0;
        _ctx.doneAirRotation = false;
        _ctx.fakeNormal = _ctx.GroundNormal;

        #endregion Misc

        #region Collision

        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.fakeNormal);
        _ctx.PlayerDirection = _ctx.transform.forward;

        #endregion Collision

        #region Velocity

        FrameworkUtility.SplitPlanarVector(_ctx.Velocity, -_ctx.Gravity.normalized, out var _v, out var _h);
        _ctx.VerticalVelocity = _v;
        _ctx.HorizontalVelocity = _h;
        _ctx.Physics_ApplyVelocity();

        InputRotations();

        #endregion Velocity
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.RingCheck();
        _ctx.HomingCheck();

        if (GroundCheck())
        {
            GroundSwitchConditions();
            return;
        }

        AirApplication(_delta);
        Gravity(_delta);
        Movement(_delta);
        RotateTowardVertical(_delta);
        DropDashCalculations(_delta);

        AirSwitchConditions();
        _ctx.Physics_ApplyVelocity();

        CheckForWallRun();

        if(_ctx.doneAirRotation) // The rotation also acts as a ledge grab timeout timer
        {
            CheckForLedgeGrab();
        }
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
    }

    #region Util

    private bool GroundCheck()
    {
        var _check = _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, _ctx.Gravity.normalized);
        _groundDetected = _check && Vector3.Dot(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) <= 0;

        if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity.normalized) > 0)
        {
            return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity.normalized) <= FrameworkUtility.SteepAngle && Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal).magnitude > _ctx.Chp.MinGroundStickSpeed;
        }
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, -_ctx.Gravity.normalized) <= FrameworkUtility.SlopeAngle;
    }

    private void Gravity(float _delta)
    {
        if (Vector3.Dot(_ctx.Velocity, _ctx.Gravity.normalized) >= _ctx.Chp.FallVelCap)
        {
            return;
        }

        if (_ctx.Jumping && _ctx.Input.JumpInput.WasReleasedThisFrame())
        {
            _ctx.Jumping = false;
            if (Vector3.Dot(_ctx.Velocity, -_ctx.Gravity) > _ctx.Chp.JumpCancel)
            {
                _ctx.VerticalVelocity = _ctx.Chp.JumpCancel * -_ctx.Gravity;
            }
            return;
        }
        _ctx.VerticalVelocity = Vector3.ClampMagnitude(_ctx.VerticalVelocity + _ctx.Chp.GravityForce * _delta * _ctx.Gravity, _ctx.Chp.FallVelCap);
    }

    private void AirApplication(float _delta)
    {
        FrameworkUtility.SplitPlanarVector(_ctx.Velocity, -_ctx.Gravity.normalized, out var _h, out var _v);
        _ctx.VerticalVelocity = _v;
        _ctx.HorizontalVelocity = _h;

        InputRotations();
        AirRotation(_delta);
    }

    private void AirRotation(float _delta)
    {
        float _turnStrength = _ctx.ChrTurn.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
        if (_ctx.InputVector.magnitude >= 0.1 && !_ctx.Skid)
        {
            _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.InputVector, _turnStrength, 0);
        }
        else if (_ctx.HorizontalVelocity.magnitude >= 0.1 && Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.PlayerDirection) > 0)
        {
            _ctx.PlayerDirection = Vector3.RotateTowards(_ctx.PlayerDirection, _ctx.HorizontalVelocity.normalized, _turnStrength, 0);
        }

        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(-_ctx.Gravity, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, -_ctx.Gravity) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, -_ctx.Gravity);

        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void Movement(float _delta)
    {
        _ctx.Skid = false;
        if (_ctx.InputVector.magnitude > 0.1)
        {
            if (Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector) < _ctx.Chp.TurnDeviationCap)
            {
                float _acceleration = _ctx.Chp.BreakStrengthAir * _delta;
                _ctx.HorizontalVelocity += _acceleration * _ctx.InputVector;

                if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.MinBreakSpeed)
                {
                    _ctx.Skid = true;
                    return;
                }
                else if (_ctx.HorizontalVelocity.magnitude < _acceleration)
                {
                    _ctx.HorizontalVelocity = _ctx.InputVector * Vector3.Dot(_ctx.InputVector, _ctx.HorizontalVelocity);
                }
            }

            if (_ctx.HorizontalVelocity.magnitude < _ctx.Chp.BaseSpeedAir)
            {
                float _acceleration = _ctx.Chp.AccelerationAir * _delta;
                _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity + (_acceleration * _ctx.InputVector), _ctx.Chp.BaseSpeedAir);
            }

            if (!FrameworkUtility.IsApproximate(_ctx.HorizontalVelocity.normalized, _ctx.InputVector, Mathf.Deg2Rad * Mathf.PI))
            {
                float _turnDeceleration = _ctx.Chp.TurnDecelerationAir.Evaluate(Vector3.Dot(_ctx.HorizontalVelocity.normalized, _ctx.InputVector)) * _delta;
                _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _turnDeceleration);
            }

            float _turnStrength = _ctx.Chp.TurnStrengthCurveAir.Evaluate(_ctx.HorizontalVelocity.magnitude) * Mathf.PI * _delta;
            _ctx.HorizontalVelocity = Vector3.RotateTowards(_ctx.HorizontalVelocity, _ctx.InputVector * _ctx.HorizontalVelocity.magnitude, _turnStrength, 0);
        }

        AirDrag(_delta);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void AirDrag(float _delta)
    {
        if (Vector3.Dot(_ctx.VerticalVelocity, -_ctx.Gravity.normalized) <= _ctx.Chp.JumpCancel)
        {
            _airDragTime = 0;
            return;
        }

        if (_ctx.HorizontalVelocity.magnitude > _ctx.Chp.BaseSpeed)
        {
            _airDragTime += _delta;
            _ctx.HorizontalVelocity = _ctx.HorizontalVelocity.normalized * Mathf.Lerp(_ctx.HorizontalVelocity.magnitude, _ctx.Chp.BaseSpeed, _ctx.Chp.AirDrag.Evaluate(_airDragTime));
        }
    }

    private void GroundSwitchConditions()
    {
        _ctx.AirDashes = 1;
        _ctx.BounceCount = 0;
        if (_ctx.DropDashing)
        {
            float _ddForce = _ctx.Chp.DropDashOutput.Evaluate(_ddchargeTime);

            if (_ctx.InputVector.magnitude > 0)
            {
                _ctx.PlayerDirection = _ctx.InputVector;
            }
            if (Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).magnitude < _ddForce)
            {
                _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.InputVector, _ctx.GroundNormal) * _ddForce;
            }

            _ctx.BounceCount = 0;
            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }

        if (_ctx.Input.CrouchInput.IsPressed() && _ctx.HorizontalVelocity.magnitude > _ctx.Chp.SpinDashInitSpeed)
        {
            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }
        _ctx.MachineTransition(PlayerStates.Ground);
    }

    private void AirSwitchConditions()
    {
        if (_ctx.HomingTargetDetector.TargetDetected && _ctx.Input.AttackInput.WasPressedThisFrame())
        {
            _ctx.Snd.PlaySound("Homing");
            _ctx.MachineTransition(PlayerStates.HomingAttack);
        }
        if (_ctx.RingDetector.TargetDetected && _ctx.Input.ReactionInput.WasPressedThisFrame())
        {
            _ctx.Snd.PlaySound("LightDash");
            _ctx.MachineTransition(PlayerStates.LightSpeedDash);
        }
        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.Snd.PlaySound("Bounce");
            _ctx.MachineTransition(PlayerStates.Bounce);
        }
        if (!_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.WasPressedThisFrame() && _ctx.AirDashes > 0)
        {
            _ctx.Snd.PlaySound("Homing");
            _ctx.AirDashes--;
            _ctx.Dash();
        }
    }

    private void DropDashCalculations(float _delta)
    {
        if (_ctx.Input.CrouchInput.IsPressed() && _ctx.Input.JumpInput.IsPressed())
        {
            _ctx.DropDashing = true;
            Debug.Log(_ddchargeTime);
            _ddchargeTime += _delta;
        }
        else
        {
            _ctx.DropDashing = false;
        }
    }

    private void CheckForWallRun()
    {
        RaycastHit hit;

        // Raycast forward from Sonic
        Vector3 origin = _ctx.Rb.transform.position;
        Vector3 dir = _ctx.PlayerDirection.normalized;

        if (Physics.Raycast(origin, dir, out hit, _ctx.Chp.WallAttachCheckDistance))
        {
            // Check that it's actually wall-like (not ground)
            // Using the surface normal angle
            float verticalDot = Mathf.Abs(hit.normal.y);

            if (verticalDot < _ctx.Chp.MinWallDot) // surface is steep enough to be a wall
            {
                // Check that Sonic is moving INTO the wall
                float intoWall = Vector3.Dot(_ctx.HorizontalVelocity.normalized, -hit.normal);

                if (intoWall > 0.2f)
                {
                    _ctx.WallRunNormal = hit.normal;
                    _ctx.OnWall = true;

                    _ctx.MachineTransition(PlayerStates.WallRun);
                    return;
                }
            }
        }

        _ctx.OnWall = false;
    }

    private void CheckForLedgeGrab() // Also logic for ledge grabbing
    {
        _ctx.ledgeGrabInitialVelocity = _ctx.Rb.linearVelocity; // Gotta save it while we have it
        _ctx.ledgeGrabStartTime = Time.time; // This too

        Vector3 vertRayStart = _ctx.ledgeVericalRayPoint.position;
        Vector3 horzRayStart = _ctx.ledgeHorizontalRayPoint.position;
        float vertLength = _ctx.ledgeVerticalRayLength;
        float horzLength = _ctx.ledgeHorizontalRayLength;
        Vector3 endPosition = new Vector3();
        Ray ray = new Ray(vertRayStart, _ctx.Gravity);
        RaycastHit hit;

        // Ray from somewhere in front of the player down, just for checking for ledges and the vertical component of the new position
        bool success = Physics.Raycast(ray, out hit, vertLength, _ctx.ledgeLayer);

        // If fails, there's no ledge to grab within range
        if (!success)
        {
            return;
        }
        Debug.DrawRay(hit.point, hit.normal, Color.yellow, 1f);

        //If the ledge is too steep, we can't grab it
        if (Vector3.Angle(-_ctx.Gravity.normalized, hit.normal) > FrameworkUtility.FloorAngle)
        {
            return;
        }

        endPosition.y = hit.point.y;
        // The relative Y value of the horizontal ledge point actually controls the displacement from the y coordinate of the ledge surface
        horzRayStart.y = hit.point.y + _ctx.ledgeHorizontalRayPoint.localPosition.y; 

        // Horizontal ray a bit down from the y position of the ledge to get the horizontal position values
        ray = new Ray(horzRayStart, _ctx.transform.forward * horzLength);
        success = Physics.Raycast(ray, out hit, horzLength, _ctx.ledgeLayer);

         // Ideally the second ray should hit the ledge but it's impossible to guarantee, so we check
        if (!success)
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan, 1f);
            return;
        }

        _ctx.ChangeKinematic(true); // Have to do this before changing the position and rotation

        //_ctx.Physics_Rotate(-hit.normal, -_ctx.Gravity); // Face the ledge
        _ctx.transform.forward = -hit.normal;

        endPosition.x = hit.point.x;
        endPosition.z = hit.point.z;

        Vector3 displacement = _ctx.ledgeGrabDisplacement; // Adjustable displacement from the ledge
        displacement.x *= _ctx.transform.forward.x;
        displacement.z *= _ctx.transform.forward.z;
        Debug.Log(_ctx.transform.forward);

        endPosition += displacement;

        //_ctx.Physics_Snap(endPosition); // Set the position
        _ctx.Rb.transform.position = endPosition;
        
        _ctx.MachineTransition(PlayerStates.LedgeGrab); // Change state
    }

    void RotateTowardVertical(float delta)
    {
        if (!_ctx.doneAirRotation && Vector3.Dot(_ctx.Gravity.normalized, _ctx.fakeNormal) < -0.995f)
        {
            _ctx.doneAirRotation = true;
            _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);
        }
        if(!_ctx.doneAirRotation)
        {
            Debug.Log("Moving!");
            _ctx.fakeNormal = Vector3.Slerp(_ctx.fakeNormal, -_ctx.Gravity.normalized, delta * _ctx.airRotationSpeed);
            _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.fakeNormal);
        }
    }
    #endregion Util
}