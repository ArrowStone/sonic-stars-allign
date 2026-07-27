using System;
using UnityEngine;

// Ledge grab, duh
public class Sonic_LedgeGrabState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;

    public Sonic_LedgeGrabState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState() // Setting the correct position and everything else is handled by AirState
    {
        _ctx.ModelManager.ExitBall();
        _ctx.Anim.SetInteger("State", 8);
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.HomingCheck();

        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            Vector3 displacement = _ctx.ledgeGrabReleaseDisplacement;
            displacement.x *= _ctx.transform.forward.x;
            displacement.z *= _ctx.transform.forward.z;
            _ctx.Rb.transform.position += displacement;

            if (_ctx.ledgeGrabVerticalVelocity.y < 0) // If players was going down, we need for them to go up after releasing the grab
            {
                _ctx.ledgeGrabVerticalVelocity = -_ctx.ledgeGrabVerticalVelocity;
            }

            // Decrease velocity over grab time
            _ctx.ledgeGrabHorizontalVelocity = _ctx.ledgeGrabHorizontalVelocity * _ctx.Chp.LedgeGrabVelocityDecrease.Evaluate(Time.time - _ctx.ledgeGrabStartTime);

            // Restore the velocity from before the grab
            _ctx.HorizontalVelocity = _ctx.ledgeGrabHorizontalVelocity;
            _ctx.VerticalVelocity = _ctx.ledgeGrabVerticalVelocity;
            _ctx.Physics_ApplyVelocity();

            _ctx.MachineTransition(PlayerStates.Air);
        }
    }

    public void LateUpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
    }
}