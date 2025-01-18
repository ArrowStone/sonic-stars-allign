using UnityEngine;

public class Sonic_HomingAttackState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _targetPos;
    private Vector3 _vel;

    public Sonic_HomingAttackState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);

        _targetPos = _ctx.HomingTargetDetector.TargetOutput.transform.position;
        _vel = _ctx.Chp.HomingAttackSpeed * (_targetPos - _ctx.transform.position).normalized;
        _ctx.PlayerDirection = _vel.normalized;
        _ctx.GroundNormal = -_ctx.Gravity;
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        if (ContinueHomingAttacking())
        {
            HomingAttackMovement(_delta);
            HomingAttackRotation();
        }
        else
        {
            ExitConditions();
        }
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Velocity = _vel;
    }

    private void HomingAttackMovement(float _delta)
    {
        _vel = _ctx.Chp.HomingAttackSpeed * (_targetPos - _ctx.transform.position).normalized;
        _ctx.Physics_Snap(_ctx.Rb.position + _vel * _delta);
    }

    private void HomingAttackRotation()
    {
        _ctx.PlayerDirection = (_targetPos - _ctx.transform.position).normalized;
        _ctx.Physics_Rotate((_targetPos - _ctx.transform.position).normalized, _ctx.GroundNormal);
    }

    private bool ContinueHomingAttacking()
    {
        if (Vector3.Dot(_targetPos - _ctx.transform.position, _vel.normalized) < 0)
        {
            return false;
        }
        return true;
    }

    public void HASwitchConditions()
    {
        if (_ctx.Input.BounceInput.WasPressedThisFrame())
        {
            _ctx.MachineTransition(PlayerStates.Bounce);
        }
    }

    private void ExitConditions()
    {
        _ctx.MachineTransition(PlayerStates.Air);
    }
}