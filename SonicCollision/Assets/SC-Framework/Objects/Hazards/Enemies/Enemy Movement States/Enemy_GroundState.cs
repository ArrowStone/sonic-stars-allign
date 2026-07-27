using UnityEngine;

public class Enemy_GroundState : IState
{
    private readonly Enemy_MovementStateMachine _etx;

    public Enemy_GroundState(Enemy_MovementStateMachine _machine)
    {
        _etx = _machine;
    }

    public void EnterState()
    {

    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        GroundSwitchConditions();
        GroundApplication(_delta);
    }

    public void LateUpdateState()
    {
        float _delta = Time.deltaTime;
    }

    public void ExitState()
    {

    }

    public void GroundApplication(float _delta)
    {
        if (_etx.GroundCast.HitInfo.normal.Equals(Vector3.zero)) return;
        _etx.GroundNormal = _etx.GroundCast.HitInfo.normal;
        _etx.HorizontalVelocity = Vector3.ProjectOnPlane(_etx.HorizontalVelocity, _etx.GroundNormal) * _etx.DragCoefficient;
        _etx.VerticalVelocity = Vector3.zero;
        _etx.Physics_ApplyVelocity();

        Vector3 targetPos = _etx.GroundCast.HitInfo.point + _etx.GroundNormal * _etx.Hover;
        _etx.Physics_Snap(targetPos);
    }

    public void GroundSwitchConditions()
    {
        if (!_etx.GroundCast.Execute(_etx.transform.position, _etx.Gravity))
        {
            _etx.MachineTransition(EnemyStates.Air);
        }
    }
}
