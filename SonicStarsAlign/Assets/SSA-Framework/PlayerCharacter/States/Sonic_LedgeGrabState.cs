using System;
using UnityEngine;

public class Sonic_LedgeGrabState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;

    public Sonic_LedgeGrabState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        //float _delta = Time.fixedDeltaTime;
    }

    public void LateUpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void ExitState()
    {
        
    }
}