﻿using UnityEngine;

public class Sonic_HomingAttackState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _targetPos;
    private Vector3 _difference;
    private Vector3 _vel;

    public Sonic_HomingAttackState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);

        _targetPos = _ctx.HomingTargetDetector.TargetOutput.transform.position;
        _difference = _targetPos - _ctx.Rb.position;
    }

    public void UpdateState()
    {
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
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

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Velocity = _vel;
    }

    private void HomingAttackMovement(float _delta)
    {
        _vel = _ctx.Chp.HomingAttackSpeed * _difference.normalized;
        _ctx.Physics_Snap(_ctx.Rb.position + Vector3.ClampMagnitude(_vel * _delta, _difference.magnitude));
    }

    private void HomingAttackRotation()
    {
        _ctx.PlayerDirection = _difference.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private bool ContinueHomingAttacking()
    {
        _difference = _targetPos - _ctx.Rb.position;
        if (_difference.magnitude <= _ctx.Rb.sleepThreshold)
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