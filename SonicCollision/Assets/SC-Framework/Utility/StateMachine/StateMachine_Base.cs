using System;
using System.Collections.Generic;

public class StateMachine_Base<EState> : object where EState : Enum
{
    public IState CurrentState { get; private set; }
    public EState CurrentEstate { get; private set; }

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

    public virtual void MachineLateUpdate()
    {
        if (!Running)
            return;

        CurrentState.LateUpdateState();
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