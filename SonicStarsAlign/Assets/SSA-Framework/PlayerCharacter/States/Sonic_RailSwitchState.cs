using UnityEngine;
using UnityEngine.Splines;

public class Sonic_RailSwitchState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private Vector3 _vel;
    private Vector3 _difference;

    private Vector3 _targetPos;
    private Vector3 _currPos;
    private Vector3 _startPos;
    private Vector3 _norm;

    private float _trgtlength;
    private float _speed;
    private float _duration;
    private float _time;
    private float _nearestTime;

    public Sonic_RailSwitchState(Sonic_PlayerStateMachine _coreMachine)
    {
        _ctx = _coreMachine;
    }

    public void EnterState()
    {
        _time = 0;
        _duration = _ctx.Chp.RailSwitchDuration;

        Setup();
        _ctx.ChangeKinematic(true);
    }

    public void UpdateState()
    {

    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        _time += _delta;
        if (ContinueRailSwitch())
        {
            RailSwitchMovement(_delta);
            RailSwitchRotation();
        }
        else
        {
            ExitConditions();
        }
        _ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, _ctx.Gravity);
    }

    public void ExitState()
    {
        _ctx.GroundNormal = _norm;
        _ctx.ChangeKinematic(false);
        _ctx.Physics_ApplyVelocity();
    }

    private void RailSwitchMovement(float _delta)
    {
        _currPos = Vector3.Lerp(_startPos, _targetPos, _time / _duration);
        _vel = (_currPos - _ctx.Rb.position) / Time.fixedDeltaTime;
        _ctx.Physics_Snap(_currPos);
    }

    private void RailSwitchRotation()
    {
        Vector3 _n = Vector3.Slerp(_ctx.GroundNormal, _norm, _time / _duration);
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _n);
    }

    private bool ContinueRailSwitch()
    {
        _difference = _targetPos - _ctx.Rb.position;
        if (_difference.magnitude <= _ctx.Rb.sleepThreshold)
        {
            return false;
        }
        return true;
    }

    private void ExitConditions()
    {
        _ctx.ChangeKinematic(false);
        _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity);
        _ctx.Physics_ApplyVelocity();
        _ctx.SplnHandler.SwitchDir.Execute(_ctx, _ctx.transform.position);
    }

    private void Setup()
    {
        SplineContainer spl = _ctx.SplnHandler.SwitchDir.RefSpline;
        _trgtlength = spl.CalculateLength();
        _startPos = _ctx.Rb.position;

        _ = SplineUtility.GetNearestPoint(spl.Spline, spl.transform.InverseTransformPoint(_startPos), out _, out _nearestTime, 10, 4);
        if (_nearestTime < 0)
        {
            _nearestTime = 0.01f / _trgtlength;
        }

        Vector3 tangent = spl.EvaluateTangent(_nearestTime).xyz;
        _speed = Vector3.Dot(_ctx.Rb.linearVelocity, tangent.normalized) / _trgtlength;
        _norm = spl.EvaluateUpVector(_nearestTime);
        _targetPos = _ctx.SplnHandler.GetPosition(spl, _nearestTime + (_duration * _speed));
    }

    public void LateUpdateState()
    {

    }
}