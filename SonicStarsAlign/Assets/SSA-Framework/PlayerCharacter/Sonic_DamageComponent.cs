using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Sonic_PlayerStateMachine))]
public class Sonic_DamageComponent : MonoBehaviour, IDamageable
{
    public Sonic_PlayerStateMachine _ctx { get; private set; }
    public Sonic_AttackComponent AttackMachine { get; private set; }
    [SerializeField] private Vector3 _bounce;
    [SerializeField] private BounceType _bounceType;

    [Space]
    [SerializeField] private float InvincibilityDuration;

    [Space]
    public GameObject Scatter;

    public float MaxRingsScattered;
    public Vector3 RingScatterVelocity;

    public UnityEvent DeathEvent;

    public event Action DealtDamage;

    public event Action PlayerDeath;

    public void DealDamage(float _damage, Vector3 _knockback, int _strength)
    {
        if (_ctx.CurrentEstate is PlayerStates.Damage or PlayerStates.Debug) return;
        if (AttackMachine.Library.Active() && AttackMachine.Library.AttackStrength() > _strength) return;

        if (_ctx.InvinciblitiyState > 0) return;
        _ctx.InvinciblitiyState = InvincibilityDuration;
        ApplyKnockback(_knockback);
        RingLoss();
    }

    public float Health()
    {
        return _ctx.Chs.Rings;
    }

    public Vector3 Bounce()
    {
        return _bounce;
    }

    public BounceType TypeBounce()
    {
        return _bounceType;
    }

    public Transform HitTransform()
    {
        return transform;
    }

    private void Awake()
    {
        _ctx = GetComponent<Sonic_PlayerStateMachine>();
        AttackMachine = GetComponent<Sonic_AttackComponent>();
    }

    private void FixedUpdate()
    {
        if (_ctx.Input.Respawn.WasPressedThisFrame())
        {
            Death();
        }

        if (_ctx.InvinciblitiyState > 0)
        {
            _ctx.InvinciblitiyState -= Time.fixedDeltaTime;
        }
    }

    public void ApplyKnockback(Vector3 _knockback)
    {
        if (_knockback.magnitude > 0.1f)
        {
            _ctx.ChangeKinematic(false);

            _ctx.Velocity = _knockback;
            _ctx.MachineTransition(PlayerStates.Damage);
        }
    }

    public void RingLoss()
    {
        if (_ctx.Death) return;
        Debug.Log("Ring loss");
        if (_ctx.Chs.Shield != null)
        {
            Debug.Log("Shield down");
            _ctx.Chs.Shield = null;
            return;
        }

        if (_ctx.Chs.Rings <= 0)
        {
            Debug.Log("Death");
            Death();
            return;
        }

        if (_ctx.Chs.Rings > 0)
        {
            Debug.Log("Confirmed ring loss");
            _ctx.Snd.PlaySound("RingScatter");

            for (var i = 1; i <= Mathf.Clamp(_ctx.Chs.Rings, 0, MaxRingsScattered); i++)
            {
                float irt = 360 / Mathf.Clamp(_ctx.Chs.Rings, 0, MaxRingsScattered) * i;
                var r = Instantiate(Scatter, transform.position, Quaternion.identity);

                r.GetComponent<ScatterCollectable>().GravityDirection = _ctx.Gravity;
                r.GetComponent<Rigidbody>().linearVelocity = Quaternion.Euler(0, irt, 0) * transform.rotation * RingScatterVelocity;
            }

            _ctx.Chs.Rings = 0;
        }

        DealtDamage?.Invoke();
    }

    public void Death()
    {
        if (_ctx.Death) { return; }

        _ctx.Death = true;
        _ctx.Invoke(nameof(_ctx.Respawn), 1);
        _ctx.MachineTransition(PlayerStates.Damage);
        //_ctx.Respawn();
        PlayerDeath?.Invoke();
        DeathEvent.Invoke();
    }
}