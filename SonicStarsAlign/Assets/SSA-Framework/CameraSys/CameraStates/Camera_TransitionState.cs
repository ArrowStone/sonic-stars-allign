﻿using UnityEngine;

public class Camera_TransitionState : IState
{
    private readonly CamBrain _ctx;

    float _time;
    float _duration;

    public Camera_TransitionState(CamBrain _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc
        _time = 0;
        _duration = _ctx.WieghtCurve.keys[_ctx.WieghtCurve.length - 1].time;
        #endregion Misc

        #region Collision
        _ctx.CashedPosition = _ctx.transform.position;
        _ctx.CashedRotation = _ctx.transform.rotation;
        #endregion Collision

        _ctx.Point.OnEnter(_ctx);
    }

    public void UpdateState()
    {
        float _delta = Time.deltaTime;
        _time += _delta;

        TransitionMovement(_delta);
        TransitionSwitchConditions();
    }

    public void FixedUpdateState()
    {
    }

    public void ExitState()
    {
    }

    public void TransitionMovement(float _delta)
    {
        _ctx.Point.Execute(_delta);
        _ctx.transform.SetPositionAndRotation(Vector3.Lerp(_ctx.CashedPosition, _ctx.Point.Position(), _ctx.WieghtCurve.Evaluate(_time)), Quaternion.Slerp(_ctx.CashedRotation, _ctx.Point.Rotation(), _ctx.WieghtCurve.Evaluate(_time)));
    }

    public void TransitionSwitchConditions()
    {
        if (_time >= _duration)
        {
            _ctx.MachineTransition(CameraStates.Alive);
        }
    }
}