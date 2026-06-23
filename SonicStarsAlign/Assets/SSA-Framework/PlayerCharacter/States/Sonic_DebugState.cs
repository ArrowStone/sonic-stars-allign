using UnityEngine;

public class Sonic_DebugState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool _groundDetected;
    private float _ddchargeTime;
    private float _airDragTime;
    private float _reactionInputTimer;

    public Sonic_DebugState(Sonic_PlayerStateMachine _machine)
    {
        _ctx = _machine;
    }

    public void EnterState()
    {
        _ctx.GroundNormal = -_ctx.Gravity.normalized;
        _ctx.Physics_Rotate(_ctx.PlayerDirection, _ctx.GroundNormal);
        _ctx.PlayerDirection = _ctx.transform.forward;

        _ctx.VerticalVelocity = Vector3.zero;
        _ctx.HorizontalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
    }

    public void UpdateState()
    {
        //float _delta = Time.deltaTime;
    }

    public void FixedUpdateState()
    {
        float _delta = Time.fixedDeltaTime;
        DebugMovement();
    }

    public void LateUpdateState()
    {
    }

    public void ExitState()
    {
    }

    private void InputRotations()
    {
        _ctx.InputRotation = Mathf.Approximately(Vector3.Angle(-_ctx.Gravity, _ctx.InputRef.up), 180)
            ? Quaternion.FromToRotation(_ctx.InputRotation * Vector3.up, -_ctx.Gravity) * _ctx.InputRotation
            : Quaternion.FromToRotation(_ctx.InputRef.up, -_ctx.Gravity);

        _ctx.InputVector = _ctx.InputRotation * _ctx.InputRef.rotation * _ctx.Input.VectorMoveInput.normalized;
    }

    private void DebugMovement()
    {
        InputRotations();
        _ctx.PlayerDirection = _ctx.InputRef.forward;
        _ = _ctx.Physics_Rotate(_ctx.PlayerDirection, -_ctx.Gravity.normalized);

        _ctx.HorizontalVelocity = _ctx.InputVector * 100f;
        if(_ctx.Input.JumpInput.IsPressed()) _ctx.VerticalVelocity = Vector3.up * 50f;
        else if(_ctx.Input.BounceInput.IsPressed()) _ctx.VerticalVelocity = Vector3.down * 50f;
        else _ctx.VerticalVelocity = Vector3.zero;
        _ctx.Physics_ApplyVelocity();
    }
}