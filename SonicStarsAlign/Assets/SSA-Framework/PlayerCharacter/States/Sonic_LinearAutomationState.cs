using UnityEngine;

public class Sonic_LinearAutomationState : IState
{

    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;    
    private Vector3 _pos;
    private bool _triggerDetected;    

    public Sonic_LinearAutomationState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);
        _ctx.GroundNormal = -_ctx.Gravity;
        _ctx.TriggerCl.TriggerEnter += TriggerCheck;
        _ctx.TriggerCl.TriggerExit += TriggerDCheck;
        _triggerDetected = false;
    }

    public void UpdateState()
    {
      
    }

    public void FixedUpdateState()
    {
        _ctx.SplnHandler.SplineMove(Time.fixedDeltaTime);
        if (_ctx.SplnHandler.Active)
        {
            SplineApplication();
            Movement();
        }

        AutomationSwitchConditions();
    }    
    
    public void ExitState()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Physics_ApplyVelocity(); 
        _ctx.TriggerCl.TriggerEnter -= TriggerCheck;
        _ctx.TriggerCl.TriggerExit -= TriggerDCheck;

        _vel = Vector3.zero;

        if (_triggerDetected) return;
        _ctx.SplnHandler.Clear();
    }

    private void Movement()
    {
        _vel = (_pos - _ctx.Rb.position) / Time.fixedDeltaTime;

        _ctx.Physics_Snap(_pos);
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private void SplineApplication()
    {   
        _pos = _ctx.SplnHandler.NewPosition();
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, -_ctx.Gravity);
        _ctx.VerticalVelocity = Vector3.Project(_vel, -_ctx.Gravity);

        if (Vector3.ProjectOnPlane(_vel, -_ctx.Gravity).magnitude > 0.1f)
        {
            _ctx.PlayerDirection = _vel.normalized;
        }
    }

    private void TriggerCheck(Collider _cl)
    {
        if (_cl == _ctx.TriggerBuffer) return;

        Debug.Log("l");
        if (_cl.TryGetComponent(out Automation_ForceSpline _s))
        {
            _s.Execute(_ctx);
            _ctx.TriggerBuffer = _cl;
            _triggerDetected = true;
            return;
        }

        if (_cl.TryGetComponent(out Automation_GrindRail _gr))
        {
            _gr.Execute(_ctx, _ctx.Rb.position);
            _ctx.TriggerBuffer = _cl;
            _triggerDetected = true;
            return;
        }
    }

    private void TriggerDCheck(Collider _)
    {
        _ctx.TriggerBuffer = null;
    }

    private void AutomationSwitchConditions()
    {
        if (_ctx.SplnHandler.Loose)
        {
            if (_ctx.Input.BounceInput.WasPressedThisFrame())
            {
                _ctx.MachineTransition(PlayerStates.Bounce);
            }
        }
        if (!_ctx.SplnHandler.Active)
        {
            _ctx.MachineTransition(PlayerStates.Air);
            return;
        }
    }
}