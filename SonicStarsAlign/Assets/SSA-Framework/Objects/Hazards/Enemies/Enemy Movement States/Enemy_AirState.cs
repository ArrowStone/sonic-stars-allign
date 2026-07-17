using UnityEngine;

public class Enemy_AirState : IState
{
    private readonly Enemy_MovementStateMachine _etx;

    public Enemy_AirState(Enemy_MovementStateMachine _machine)
    {
        _etx = _machine;
    }

    public void EnterState()
    {
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime; ;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;


        AirSwitchConditions();
        AirMovement(_delta);
    }

    public void LateUpdateState()
    {
        float _delta = Time.deltaTime;
    }

    public void ExitState()
    {

    }

    public void AirMovement(float _delta)
    {
        _etx.HorizontalVelocity *= _etx.DragCoefficient;
        _etx.VerticalVelocity += _etx.Gravity;
        if (_etx.VerticalVelocity.magnitude > _etx.FallSpeedCap)
            _etx.VerticalVelocity = _etx.VerticalVelocity.normalized * _etx.FallSpeedCap;
        _etx.Physics_ApplyVelocity();
    }

    public void AirSwitchConditions()
    {
        if (_etx.GroundCast.Execute(_etx.transform.position, _etx.Gravity))
        {
            _etx.MachineTransition(EnemyStates.Ground);
        }
    }
}
