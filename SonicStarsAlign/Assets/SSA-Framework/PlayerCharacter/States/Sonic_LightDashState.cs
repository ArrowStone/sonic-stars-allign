using UnityEngine;

public class Sonic_LightDashState : IState
{
    public Sonic_PlayerStateMachine _ctx;

    private Vector3 _targetPos;
    private Vector3 _vel;

    public Sonic_LightDashState(Sonic_PlayerStateMachine _coreMachine)
    {
        _ctx = _coreMachine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);

        _targetPos = _ctx.RingDetector.TargetOutput.transform.position;
        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _vel = _ctx.Chp.LightDashSpeed * (_targetPos - _ctx.transform.position).normalized;
        _ctx.PlayerDirection = _vel.normalized;
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        if (!ContinueLightDashing())
        {
            AirSwitchConditions();
            return;
        }

        _targetPos = _ctx.RingDetector.TargetOutput.transform.position;
        LightDashMovement(_delta);
        LightDashRotation();
        LightSwitchConditions();
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Velocity = _vel;
    }

    private void LightDashMovement(float _delta)
    {
        _vel = _ctx.Chp.LightDashSpeed * (_targetPos - _ctx.Rb.position).normalized;
        _ctx.Physics_Snap(_ctx.Rb.position + _vel * _delta);
    }

    private void LightDashRotation()
    {
        _ctx.PlayerDirection = _vel.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity);
    }

    private bool ContinueLightDashing()
    {
        if (_ctx.RingDetector.TargetOutput == null || Vector3.Dot(_targetPos - _ctx.Rb.position, _vel.normalized) < 0)
        {
            _ctx.RingCheck();
            if (_ctx.RingDetector.TargetOutput == null || Vector3.Dot(_ctx.RingDetector.TargetOutput.transform.position - _ctx.Rb.position, _vel.normalized) < 0)
            {
                return false;
            }
        }
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