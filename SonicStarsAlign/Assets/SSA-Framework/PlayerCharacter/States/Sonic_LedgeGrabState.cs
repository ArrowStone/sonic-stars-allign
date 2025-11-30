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
        Debug.Log("Grab!");
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        //float _delta = Time.fixedDeltaTime;
        if(_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            Debug.Log("Ungrabbing!");

            Vector3 displacement = _ctx.ledgeGrabReleaseDisplacement;
            displacement.x *= _ctx.transform.forward.x;
            displacement.z *= _ctx.transform.forward.z;
            _ctx.transform.position += displacement;

            if(_ctx.ledgeGrabInitialVelocity.y < 0) // If players was going down, we need for them to go up after releasing the grab
            {
                _ctx.ledgeGrabInitialVelocity.y = -_ctx.ledgeGrabInitialVelocity.y;
            }

            _ctx.ChangeKinematic(false);

            // Decrease velocity over grab time
            _ctx.ledgeGrabInitialVelocity = _ctx.ledgeGrabInitialVelocity * _ctx.LedgeGrabVelocityDecrease.Evaluate(Time.time - _ctx.ledgeGrabStartTime);
            _ctx.Rb.linearVelocity = _ctx.ledgeGrabInitialVelocity; // Restore the velocity from before the grab
            Debug.Log(_ctx.ledgeGrabInitialVelocity);
            
            _ctx.MachineTransition(PlayerStates.Air);
        }
    }

    public void LateUpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void ExitState()
    {
        
    }
}