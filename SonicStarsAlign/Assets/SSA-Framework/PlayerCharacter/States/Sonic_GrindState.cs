using System;
using UnityEngine;

public class Sonic_GrindState : IState
{
    public Sonic_PlayerStateMachine _ctx;
    private bool _collided;
    private Vector3 _difference;
    private float _dotP;
    private Vector3 _norm;
    private Vector3 _pos;
    private Vector3 _vel;
    private float _railTurn;
    private float _tilt;

    public Sonic_GrindState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
        _ctx.railEndTime = Time.time;
    }

    public void EnterState()
    {
        //Debug.DrawRay(_ctx.transform.position, Vector3.up * 5f, Color.white, 10f);
        _ctx.ModelManager.ExitBall();
        _ctx.Anim.SetInteger("State", 2);

        _ctx.ChangeKinematic(true);
        _difference = Vector3.Project(_ctx.Velocity, _ctx.SplnHandler.SplineTangent());
        _ctx.VerticalVelocity = Vector3.zero;

        _ctx.Snd.PlaySound("RailLand");

        _ctx.Snd.RailSpinSource.clip = _ctx.Snd.railGrindSound;
        _ctx.Snd.RailSpinSource.Play();

        RailApplication();
    }

    public void ExitState()
    {
        Debug.DrawRay(_ctx.transform.position, Vector3.up * 5f, Color.black, 10f);

        _ctx.ChangeKinematic(false);
        _ctx.HorizontalVelocity = _vel;
        _ctx.Physics_ApplyVelocity();
        _ctx.SplnHandler.Clear();

        _vel = Vector3.zero;

        _ctx.Snd.RailSpinSource.Stop();
        _ctx.Snd.RailSpinSource.pitch = 1f;

        _ctx.railEndTime = Time.time;
        _ctx.Snd.RailSpinSource.volume = 0.7f;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;

        _ctx.SplnHandler.SplineMove(_delta);
        if (_ctx.SplnHandler.Active)
        {
            RailApplication();
            InputRotations();
            if (!CollisionD(_delta))
            {
                Movement(_delta);
                Rotation();
                SlopePhysics(_delta);
            }
        }
        _ctx.RailCheck();
        RailSwitchConditions();

        _ctx.Snd.RailSpinSource.pitch = Mathf.Lerp(_ctx.Snd.RailSpinSource.pitch, Math.Clamp(_ctx.Rb.linearVelocity.magnitude * 0.05f, 0.8f, 1.2f), _delta * 10f);
        _ctx.Snd.RailSpinSource.volume = Math.Clamp(_ctx.Rb.linearVelocity.magnitude * 0.03f, 0.1f, 0.7f);
    }

    public void LateUpdateState()
    {
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    #region Util
    // Making it a separate state is overkill IMO
    void Trick()
    {
        _ctx.Snd.PlaySound("RailLand");
        _ctx.Anim.SetInteger("State", 100);
        _ctx.Anim.SetTrigger("RailTrick");

        if (_ctx.SplnHandler.SpeedMultiplier < _ctx.Chp.RailTrickSpeed)
        {
            _ctx.SplnHandler.SpeedMultiplier = _ctx.Chp.RailTrickSpeed * (_ctx.SplnHandler.Forward ? 1 : -1);
            _ctx.SplnHandler.SpeedFactor = _ctx.SplnHandler.SpeedMultiplier;
        }
    }

    public void RailSwitchConditions()
    {
        if (_ctx.Input.JumpInput.WasPressedThisFrame())
        {
            //_ctx.Anim.SetInteger("State", 1);
            Automation_GrindRail _grail;
            if (_tilt < -_ctx.Chp.RailSwitchDeadZone && _ctx.RailDetectorL.TargetDetected)
            {
                if (_ctx.RailDetectorL.TargetOutput.TryGetComponent(out _grail))
                {
                    _ctx.SplnHandler.SwitchDir = _grail;
                    _ctx.MachineTransition(PlayerStates.RailSwitch);
                    return;
                }
            }
            if (_tilt > _ctx.Chp.RailSwitchDeadZone && _ctx.RailDetectorR.TargetDetected)
            {
                if (_ctx.RailDetectorR.TargetOutput.TryGetComponent(out _grail))
                {
                    _ctx.SplnHandler.SwitchDir = _grail;
                    _ctx.MachineTransition(PlayerStates.RailSwitch);
                    return;
                }
            }
            _ctx.Jump();
        }
        if (_ctx.Input.AttackInput.WasPressedThisFrame())
        {
            Trick();
        }
        if (!_ctx.SplnHandler.Active)
        {
            _ctx.Anim.SetInteger("State", 0);
            if (_ctx.GroundCast.Execute(_ctx.Rb.transform.position, -_ctx.GroundNormal))
            {
                _ctx.MachineTransition(PlayerStates.Ground);
                return;
            }
            _ctx.MachineTransition(PlayerStates.Air);
        }
    }

    private bool CollisionD(float _delta)
    {
        _collided = _ctx.Physics_Sweep(_ctx.SplnHandler.NewPosition(), out _);

        if (_collided)
        {
            _ctx.SplnHandler.SpeedMultiplier = _ctx.SplnHandler.Forward ? -_ctx.Chp.RailCollisionBounce : _ctx.Chp.RailCollisionBounce;
            _ctx.SplnHandler.SplineMove(_delta);
        }
        return _collided;
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(_ctx.GroundNormal, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, _ctx.GroundNormal) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, _ctx.GroundNormal);

        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void Movement(float _delta)
    {
        _vel = _difference / _delta;
        _ctx.Physics_Snap(_pos);

        _ctx.Anim.SetFloat("TILT", _tilt);
        Debug.Log(_tilt);
    }

    private void RailApplication()
    {
        _norm = _ctx.SplnHandler.SplineNormal();
        _ctx.GroundNormal = _norm;

        //_ctx.HorizontalVelocity = Vector3.ProjectOnPlane(_vel, _ctx.GroundNormal);
        _pos = _ctx.SplnHandler.NewPosition();
        _difference = _pos - _ctx.transform.position;

        _tilt = Vector3.SignedAngle(_ctx.PlayerDirection, _ctx.InputVector, _ctx.GroundNormal) * 0.01111111111f;
        _tilt += Vector3.SignedAngle(_ctx.PlayerDirection, _difference.normalized, _ctx.GroundNormal);
        _tilt = Math.Clamp(_tilt, -1f, 1f);
        _railTurn = _ctx.SplnHandler.SplineCurvature();
    }

    private void Rotation()
    {
        if (_difference.magnitude >= _ctx.Rb.sleepThreshold)
        {
            _ctx.PlayerDirection = _difference.normalized;
        }
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
    }

    private void SlopePhysics(float _delta)
    {
        if (_collided)
        {
            return;
        }

        _dotP = Vector3.Dot(_ctx.SplnHandler.SplineTangent().normalized, _ctx.Gravity);
        if (_ctx.Input.CrouchInput.IsPressed())
        {
            if ((_ctx.SplnHandler.Forward && _dotP > 0) || (!_ctx.SplnHandler.Forward && _dotP < 0))
            {
                _ctx.SplnHandler.SpeedMultiplier += _dotP * _ctx.Chp.RailCrouchInfluence * _delta;
            }
            else
            {
                _ctx.SplnHandler.SpeedMultiplier += _dotP * _ctx.Chp.RailSlopeInfluence * _delta;
            }
            return;
        }
        _ctx.SplnHandler.SpeedMultiplier += _dotP * _ctx.Chp.RailSlopeInfluence * _delta;
        _ctx.SplnHandler.SpeedMultiplier = Mathf.Clamp(_ctx.SplnHandler.SpeedMultiplier, -_ctx.Chp.RailSpeedCap, _ctx.Chp.RailSpeedCap);
    }

    #endregion Util
}