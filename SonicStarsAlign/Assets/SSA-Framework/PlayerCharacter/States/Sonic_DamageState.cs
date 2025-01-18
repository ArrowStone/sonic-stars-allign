using UnityEngine;

public class Sonic_DamageState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;

    public Sonic_DamageState(Sonic_PlayerStateMachine _coreMachine)
    {
        _ctx = _coreMachine;
    }

    public void EnterState()
    {
        _groundDetected = false;
        _ctx.GroundNormal = -_ctx.Gravity.normalized;
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;

        if (GroundCheck())
        {
        }
        else
        {
            Gravity(_delta);
            HurtMovement(_delta);
            HurtRotation();
        }
        _ctx.Physics_ApplyVelocity();
        HurtSwitchConditions();
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
    }

    private void HurtMovement(float _delta)
    {
        _ctx.HorizontalVelocity =
            Vector3.MoveTowards(_ctx.HorizontalVelocity, Vector3.zero, 20 * _delta);
    }

    private void HurtRotation()
    {
        if (_ctx.HorizontalVelocity.magnitude > 0.1)
        {
            _ctx.PlayerDirection = -_ctx.HorizontalVelocity.normalized;
        }

        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity);
    }

    private bool GroundCheck()
    {
        var _check = _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, _ctx.Gravity.normalized) || _ctx.GroundCast.Execute(_ctx.Rb.worldCenterOfMass, -_ctx.Gravity.normalized);
        _groundDetected = _check && Vector3.Dot(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) <= 0;
        return _groundDetected;
    }

    private void Gravity(float _delta)
    {
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_ctx.Velocity, -_ctx.Gravity);
        _ctx.VerticalVelocity = Vector3.Project(_ctx.Velocity, -_ctx.Gravity);
        if (Vector3.Dot(_ctx.VerticalVelocity, _ctx.Gravity) > 0 && _ctx.VerticalVelocity.magnitude >= _ctx.Chp.FallVelCap)
        {
            return;
        }

        _ctx.VerticalVelocity += _ctx.Chp.GravityForce * _delta * _ctx.Gravity;
    }

    private void HurtSwitchConditions()
    {
        if (_groundDetected && Vector3.Dot(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal) < 0)
        {
            if (_ctx.Death)
            {
                _ctx.Velocity = Vector3.zero;
                return;
            }
            if (Vector3.ProjectOnPlane(_ctx.Velocity, -_ctx.Gravity).magnitude > 20)
            {
                _ctx.Velocity = Vector3.ProjectOnPlane(_ctx.Velocity, _ctx.GroundCast.HitInfo.normal);
                return;
            }
            _ctx.MachineTransition(PlayerStates.Ground);
        }
    }
}