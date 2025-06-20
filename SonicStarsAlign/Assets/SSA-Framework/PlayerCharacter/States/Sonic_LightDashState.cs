using UnityEngine;

public class Sonic_LightDashState : IState
{
    public Sonic_PlayerStateMachine _ctx;

    private Vector3 _targetPos;
    private Vector3 _difference;
    private Vector3 _vel;

    public Sonic_LightDashState(Sonic_PlayerStateMachine _coreMachine)
    {
        _ctx = _coreMachine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);
        _ctx.GroundNormal = -_ctx.Gravity.normalized;
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        if (!ContinueLightDashing())
        {
            AirSwitchConditions();
            return;
        }

        LightDashMovement(_delta);
        LightDashRotation();
        LightSwitchConditions();
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Velocity = _vel;
    }

    private void LightDashMovement(float _delta)
    {
        _vel = _difference.normalized * _ctx.Chp.LightDashSpeed;
        _ctx.Physics_Snap(_ctx.Rb.position + Vector3.ClampMagnitude(_vel * _delta, _difference.magnitude));
    }

    private void LightDashRotation()
    {
        _ctx.PlayerDirection = _difference.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity);
    }

    private bool ContinueLightDashing()
    {
        _difference = _targetPos - _ctx.Rb.position;
        if (_ctx.RingDetector.TargetOutput == null || _difference.magnitude <= _ctx.Rb.sleepThreshold)
        {
            _ctx.RingCheck();
            if (_ctx.RingDetector.TargetOutput == null || Vector3.Dot(_ctx.RingDetector.TargetOutput.transform.position - _ctx.Rb.position, _vel.normalized) <= 0)
            {
                return false;
            }
        }
        _targetPos = _ctx.RingDetector.TargetOutput.transform.position;
        _difference = _targetPos - _ctx.Rb.position;
        return true;
    }

    private void LightSwitchConditions()
    {
        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Bounce);
        }
    }

    private void AirSwitchConditions()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }
}