using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_MonoBase<EState> : MonoBehaviour where EState : Enum
{
    public IState CurrentState { get; protected set; }
    public EState CurrentEstate { get; protected set; }

    private bool _running;

    public bool Running
    {
        get
        {
            return _running;
        }
        set
        {
            _running = value;
        }
    }

    protected Dictionary<EState, IState> States = new();

    public event Action<EState> StateChanged;

    public virtual void Initialize()
    {
        Running = true;
    }

    public virtual void MachineUpdate()
    {
        if (!Running)
            return;

        CurrentState.UpdateState();
    }

    public virtual void MachineFixedUpdate()
    {
        if (!Running)
            return;

        CurrentState.FixedUpdateState();
    }

    public void MachineTransition(EState _nextState)
    {
        StateChanged?.Invoke(_nextState);

        CurrentState.ExitState();
        CurrentEstate = _nextState;
        CurrentState = States[_nextState];
        CurrentState.EnterState();
    }
}