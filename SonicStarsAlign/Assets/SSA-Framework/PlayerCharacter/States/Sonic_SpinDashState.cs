using UnityEngine;

public class Sonic_SpinDashState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private int _spincharge;

    public Sonic_SpinDashState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        Debug.Log("rev");

        #region Misc

        _groundDetected = true;
        _ctx.Skid = false;
        _spincharge = 0;

        #endregion Misc

        #region Collision

        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundCast.HitInfo.normal);

        #endregion Collision

        #region Velocity

        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal);
        _ctx.Physics_ApplyVelocity();

        InputRotations();

        #endregion Velocity
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        if (!GroundCheck())
        {
            AirSwitchConditions();
            return;
        }

        GroundApplication(_delta);
        SlopePhysics(_delta);
        Movement(_delta);

        GroundSwitchConditions();
        _ctx.Physics_ApplyVelocity();
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
    }

    public void ExitState()
    {
    }

    #region Util

    private bool GroundCheck()
    {
        _groundDetected = _ctx.GroundCast.Execute(_ctx.Rb.position, -_ctx.GroundNormal);
        return _groundDetected && Vector3.Angle(_ctx.GroundCast.HitInfo.normal, _ctx.GroundNormal) <= _ctx.Chp.MaxGroundDeviation;
    }

    private void GroundApplication(float _delta)
    {
        _ctx.GroundNormal = _ctx.GroundCast.HitInfo.normal;
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundNormal).normalized * _ctx.Velocity.magnitude;
        _ctx.Physics_Snap(_ctx.GroundCast.HitInfo.point + _ctx.GroundNormal * _ctx.PlayerHover, _ctx.SnapForce * _delta);

        InputRotations();
        GroundRotation(_delta);
    }

    private void GroundRotation(float _delta)
    {
        if (_ctx.InputVector.magnitude >= 0.1)
        {
            _ctx.PlayerDirection = _ctx.InputVector;
        }
        _ctx.PlayerDirection = Vector3.ProjectOnPlane(_ctx.PlayerDirection, _ctx.GroundNormal).normalized;

        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private void Movement(float _delta)
    {
        float _deceleration = _ctx.Chp.SpinDashDeceleration * _delta;
        _ctx.HorizontalVelocity = Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, _deceleration);
        _ctx.HorizontalVelocity = Vector3.ClampMagnitude(_ctx.HorizontalVelocity, _ctx.Chp.HardSpeedCap);
    }

    private void SlopePhysics(float _delta)
    {
        if (Vector3.Angle(-_ctx.Gravity, _ctx.GroundNormal) <= FrameworkUtility.FloorAngle) return;

        float _slopeFactor = _ctx.Chp.SlopeFactor * _delta;
        _ctx.HorizontalVelocity += Vector3.ProjectOnPlane(_ctx.Gravity, _ctx.GroundNormal) * _slopeFactor;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);

        //  Quaternion cameraRotation = Quaternion.Euler(0, StateMachine.Camera.eulerAngles.y, 0);
        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void GroundSwitchConditions()
    {
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            _spincharge++;
        }
        if (_ctx.Input.CrouchInput.WasReleasedThisFrame())
        {
            _ctx.Velocity = _ctx.PlayerDirection * _ctx.Chp.SpinDashOutput.Evaluate(_spincharge);
            _ctx.MachineTransition(PlayerStates.Roll);
            return;
        }
    }

    private void AirSwitchConditions()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }

    #endregion Util
}