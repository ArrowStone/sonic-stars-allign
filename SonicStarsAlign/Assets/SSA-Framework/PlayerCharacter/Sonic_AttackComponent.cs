using UnityEngine;

[RequireComponent(typeof(Sonic_PlayerStateMachine))]
public class Sonic_AttackComponent : MonoBehaviour
{
    private Sonic_PlayerStateMachine ctx;
    public AttackLibrary Library;
    public Vector3 RecoilVector;
    public int comboLength;

    private void OnEnable()
    {
        #region ComponentSetup

        Library.Initialize();
        ctx = GetComponent<Sonic_PlayerStateMachine>();
        ctx.StateChanged += StateResponce;

        #endregion ComponentSetup

        #region AttackSetup

        foreach (Attack attack in Library.AttacksLibrary)
        {
            if (attack == Library.GetAttack("StompAOE")) return;

            attack.HitAction += HitAction;
        }
        ctx.JumpAction += JumpAttack;

        #endregion AttackSetup
    }

    private void OnDisable()
    {
        ctx.StateChanged -= StateResponce;
        foreach (Attack attack in Library.AttacksLibrary)
        {
            attack.HitAction -= HitAction;
        }
    }

    private bool jumping;

    private void FixedUpdate()
    {
        Library.AttackUpdate(Time.fixedDeltaTime);

        if (!ctx.Jumping && jumping && ctx.CurrentEstate != PlayerStates.Air)
        {
            Library.StopAttack("Spin");
            jumping = false;
        }

        if(ctx.CurrentEstate == PlayerStates.Ground)
        {
            if (comboLength > 0) Debug.Log("Combo of length " + comboLength);

            int scoreAdd = 0;

            if(comboLength > 2) scoreAdd = 200;
            if(comboLength > 4) scoreAdd = 700;
            if(comboLength > 6) scoreAdd = 1000;

            ctx.Chs.Score += scoreAdd;
            comboLength = 0;
        }
    }

    public void JumpAttack()
    {
        Library.StartAttack("Spin");
        jumping = true;
    }

    public void StateResponce(PlayerStates _playerState)
    {
        Library.StopAttack("Roll");
        Library.StopAttack("SDash");
        Library.StopAttack("Bounce");
        Library.StopAttack("HAttack");
        Library.StopAttack("SweepKick");

        switch (_playerState)
        {
            case PlayerStates.HomingAttack:
                {
                    Library.StartAttack("HAttack");
                    break;
                }
            case PlayerStates.Roll:
                {
                    Library.StartAttack("Roll");
                    break;
                }
            case PlayerStates.Spindash:
                {
                    Library.StartAttack("SDash");
                    break;
                }
            case PlayerStates.Bounce:
                {
                    Library.StartAttack("Bounce");
                    break;
                }
            case PlayerStates.SweepKick:
                {
                    Library.StartAttack("SweepKick");
                    break;
                }
        }
    }

    private void HitAction(IDamageable _damageable)
    {
        if (_damageable is ItemBox)
        {
            var i = _damageable as ItemBox;
            i.Ctx = ctx;
        }
        if (ctx.CurrentEstate is PlayerStates.Ground or PlayerStates.Roll or PlayerStates.Spindash)
        {
            return;
        }

        comboLength += 1;

        if (ctx.CurrentEstate is PlayerStates.HomingAttack)
        {
            ctx.MachineTransition(PlayerStates.Air);
            if (_damageable.Health() > Library.GetAttack("HAttack").Damage)
            {
                Recoil();
                return;
            }
        }

        if (ctx.CurrentEstate is PlayerStates.Bounce)
        {
            if (_damageable.Health() > Library.GetAttack("Bounce").Damage)
            {
                Recoil();
            }
            return;
        }
        Bounce(_damageable);
    }

    private void Bounce(IDamageable _damageable)
    {
        switch (_damageable.TypeBounce())
        {
            case BounceType.BounceWorld:
                {
                    ctx.Velocity = Vector3.ProjectOnPlane(ctx.Rb.linearVelocity, _damageable.Bounce().normalized) + _damageable.Bounce();
                    break;
                }
            case BounceType.BounceLocal:
                {
                    ctx.Velocity = Vector3.ProjectOnPlane(ctx.Rb.linearVelocity, _damageable.HitTransform().rotation * _damageable.Bounce().normalized) + (_damageable.HitTransform().rotation * _damageable.Bounce());
                    break;
                }
            case BounceType.LockWorld:
                {
                    ctx.Velocity = _damageable.Bounce();
                    break;
                }
            case BounceType.LockLocal:
                {
                    ctx.Velocity = _damageable.HitTransform().rotation * _damageable.Bounce();
                    break;
                }
        }
    }

    private void Recoil()
    {
        ctx.MachineTransition(PlayerStates.Air);
        ctx.Rb.linearVelocity = ctx.transform.rotation * RecoilVector;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (Attack attack in Library.AttacksLibrary)
        {
            Gizmos.DrawSphere(transform.position + (transform.rotation * attack.Offset), attack.radius);
        }
    }
}