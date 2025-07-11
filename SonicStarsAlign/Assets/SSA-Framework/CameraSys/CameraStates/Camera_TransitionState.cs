﻿using UnityEngine;

public class Camera_TransitionState : IState
{
    private readonly CamBrain _ctx;

    private float _time;
    private float _duration;

    public Camera_TransitionState(CamBrain _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        #region Misc

        _time = 0;
        _duration = _ctx.WeightCurve.keys[_ctx.WeightCurve.length - 1].time;

        #endregion Misc

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

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
        _ctx.CashedTransform = new()
        {
            Position = _ctx.Cam.transform.position,
            Rotation = _ctx.Cam.transform.rotation,
        };
    }

    public void TransitionMovement(float _delta)
    {
        _ctx.Point.Execute(_delta);
        var _transfrm = _ctx.Point.Transform();
        _ctx.transform.SetPositionAndRotation(Vector3.Lerp(_ctx.CashedTransform.Position, _transfrm.Position, _ctx.WeightCurve.Evaluate(_time)), Quaternion.Slerp(_ctx.CashedTransform.Rotation, _transfrm.Rotation, _ctx.WeightCurve.Evaluate(_time)));
    }

    public void TransitionSwitchConditions()
    {
        if (_time >= _duration)
        {
            _ctx.MachineTransition(CameraStates.Alive);
        }
    }
}