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
        _ctx.Anim.SetInteger("State", 4);
        _ctx.Snd.PlaySound("LightDash");
        _ctx.VerticalVelocity = Vector3.zero;
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        if (!ContinueLightDashing(_delta))
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
    }

    private void LightDashMovement(float _delta)
    {
        _vel = _difference.normalized * _ctx.Chp.LightDashSpeed;
        _ctx.Physics_Snap(_ctx.transform.position + Vector3.ClampMagnitude(_vel * _delta, _difference.magnitude));
        _ctx.HorizontalVelocity = _vel;
        _ctx.Physics_ApplyVelocity();
    }

    private void LightDashRotation()
    {
        _ctx.PlayerDirection = _difference.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity);
    }

    private bool ContinueLightDashing(float _delta)
    {
        _difference = _targetPos - _ctx.Rb.transform.position;

        if (_ctx.RingDetector.TargetOutput == null || _difference.magnitude <= _ctx.Rb.sleepThreshold)
        {
            _ctx.RingCheck();
            if (_ctx.RingDetector.TargetOutput == null || Vector3.Dot(_ctx.RingDetector.TargetOutput.transform.position - _ctx.Rb.transform.position, _vel.normalized) <= 0)
            {
                return false;
            }

        }
        
        _targetPos = _ctx.RingDetector.TargetOutput.transform.position;
        _difference = _targetPos - _ctx.Rb.transform.position;

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
        // If the player wants to stop
        if(_ctx.InputVector.magnitude != 0)
        {
            _ctx.Velocity = _ctx.PlayerDirection * _ctx.Chp.LightDashExitSpeed;
        }
        else
        {
            _ctx.Velocity = Vector3.zero;
        }

        _ctx.Anim.SetInteger("State", 0);
        _ctx.MachineTransition(PlayerStates.Air);
    }
}