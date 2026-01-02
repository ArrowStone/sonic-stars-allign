using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Sonic_PlayerStateMachine))]
public class Sonic_DamageComponent : MonoBehaviour, IDamageable
{
    public Sonic_PlayerStateMachine CTX { get; private set; }
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
        Debug.Log("Damage");
        if (AttackMachine.Library.Active() && AttackMachine.Library.AttackStrength() > _strength) return;
        ApplyKnockback(_knockback);

        if (CTX.InvinciblitiyState > 0) return;
        CTX.InvinciblitiyState = InvincibilityDuration;
        RingLoss();
    }

    public float Health()
    {
        return CTX.Chs.Rings;
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
        CTX = GetComponent<Sonic_PlayerStateMachine>();
        AttackMachine = GetComponent<Sonic_AttackComponent>();
    }

    private void FixedUpdate()
    {
        if (CTX.InvinciblitiyState > 0)
        {
            CTX.InvinciblitiyState -= Time.fixedDeltaTime;
            Debug.Log(CTX.InvinciblitiyState);
        }
    }

    public void ApplyKnockback(Vector3 _knockback)
    {
        if (_knockback.magnitude > 0.1f)
        {
            CTX.ChangeKinematic(false);

            CTX.Velocity = _knockback;
            CTX.MachineTransition(PlayerStates.Damage);
        }
    }

    public void RingLoss()
    {
        Debug.Log("Ring loss");
        if (CTX.Chs.Shield != null)
        {
            Debug.Log("Shield down");
            CTX.Chs.Shield = null;
            return;
        }

        if (CTX.Chs.Rings <= 0)
        {
            Debug.Log("Death");
            Death();
            return;
        }

        if (CTX.Chs.Rings > 0)
        {
            Debug.Log("Confirmed ring loss");
            CTX.Snd.PlaySound(CTX.Snd.ringScatterSound);

            for (var i = 1; i <= Mathf.Clamp(CTX.Chs.Rings, 0, MaxRingsScattered); i++)
            {
                float irt = 360 / Mathf.Clamp(CTX.Chs.Rings, 0, MaxRingsScattered) * i;
                var r = Instantiate(Scatter, transform.position, Quaternion.identity);

                r.GetComponent<ScatterCollectable>().GravityDirection = CTX.Gravity;
                r.GetComponent<Rigidbody>().linearVelocity = Quaternion.Euler(0, irt, 0) * transform.rotation * RingScatterVelocity;
            }

            CTX.Chs.Rings = 0;
        }

        DealtDamage?.Invoke();
    }

    public void Death()
    {
        if (CTX.Death) { return; }

        CTX.Death = true;
        CTX.MachineTransition(PlayerStates.Damage);
        CTX.Invoke(nameof(CTX.Respawn), 1);
        PlayerDeath?.Invoke();
        DeathEvent.Invoke();
    }
}