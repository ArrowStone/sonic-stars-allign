using UnityEngine;

public class Sonic_WallRunState : IState
{
    private readonly Sonic_PlayerStateMachine _ctx;
    private bool isChargingJump;
    private float chargeTime;

    public Sonic_WallRunState(Sonic_PlayerStateMachine machine)
    {
        _ctx = machine;
    }

    public void EnterState()
    {
        _ctx.Rb.useGravity = false;
        _ctx.VerticalVelocity = Vector3.zero;
        chargeTime = 0f;
        isChargingJump = false;
    }

    public void UpdateState()
    {
        // Check for continued wall contact
        if (!IsTouchingWall())
        {
            ExitToAir();
            return;
        }

        // Begin charging wall jump
        if (_ctx.Input.JumpInput.IsPressed())
        {
            isChargingJump = true;
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, _ctx.Chp.MaxWallJumpChargeTime);
        }

        // Release jump to perform jump off wall
        if (_ctx.Input.JumpInput.WasReleasedThisFrame() && isChargingJump)
        {
            PerformWallJump();
            return;
        }

        // Small "cling" effect � slows descent if not jumping
        _ctx.Rb.linearVelocity = Vector3.Lerp(_ctx.Rb.linearVelocity, Vector3.zero, Time.deltaTime * 4f);
    }

    public void FixedUpdateState() { }

    public void LateUpdateState() { }

    public void ExitState()
    {
        _ctx.Rb.useGravity = true;
    }

    private bool IsTouchingWall()
    {
        return Physics.Raycast(_ctx.transform.position, _ctx.transform.forward,
            out RaycastHit hit, _ctx.Chp.WallAttachCheckDistance, _ctx.Chp.WallLayer);
    }

    private void PerformWallJump()
    {
        Vector3 jumpDir = (_ctx.transform.forward + _ctx.WallNormal).normalized;
        float chargeRatio = chargeTime / _ctx.Chp.MaxWallJumpChargeTime;

        _ctx.Rb.useGravity = true;
        _ctx.Rb.linearVelocity = jumpDir * (_ctx.Chp.WallJumpForce * chargeRatio);

        _ctx.MachineTransition(PlayerStates.Air);
    }

    private void ExitToAir()
    {
        _ctx.Rb.useGravity = true;
        _ctx.MachineTransition(PlayerStates.Air);
    }
}