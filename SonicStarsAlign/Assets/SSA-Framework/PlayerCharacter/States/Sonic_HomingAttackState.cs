using UnityEngine;

public class Sonic_HomingAttackState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Transform _targetTransform;
    private bool homingOntoSpline;
    private Vector3 _difference;
    private Vector3 _vel;

    public Sonic_HomingAttackState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.ChangeKinematic(true);

        _targetTransform = _ctx.HomingTargetDetector.TargetOutput.transform;
        homingOntoSpline = _ctx.homingOntoSpline;
        _difference = _ctx.homingTargetPosition - _ctx.transform.position;

        _ctx.Anim.SetInteger("State", 1);
        _ctx.ModelManager.EnterBall();
        _ctx.ModelManager.ballRollSpeed = 40f;
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
            HASwitchConditions();
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
        _ctx.AirDashes = 1;
    }

    private void HomingAttackMovement(float _delta)
    {
        _vel = _ctx.Chp.HomingAttackSpeed * _difference.normalized;
        _ctx.Physics_Snap(_ctx.Rb.transform.position + Vector3.ClampMagnitude(_vel * _delta, _difference.magnitude));
    }

    private void HomingAttackRotation()
    {
        _ctx.PlayerDirection = _difference.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private bool ContinueHomingAttacking()
    {
        if (!homingOntoSpline) _ctx.homingTargetPosition = _targetTransform.position;
        _difference = _ctx.homingTargetPosition - _ctx.transform.position;
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
        _ctx.ChangeKinematic(false);
        _ctx.VerticalVelocity = -_ctx.Gravity * _ctx.Chp.HomingAttackBounceForce;
        //_ctx.HorizontalVelocity = Vector3.zero;
        /*if(_ctx.Input.AttackInput.IsPressed())
            _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, -_ctx.Gravity);*/

        _ctx.Physics_ApplyVelocity();

        _ctx.InvinciblitiyState = 0.5f;

        _ctx.MachineTransition(PlayerStates.Air);
    }
}