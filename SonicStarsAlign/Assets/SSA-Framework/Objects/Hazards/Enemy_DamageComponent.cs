using UnityEngine;
using UnityEngine.Events;

public class Enemy_DamageComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private Vector3 bounce;
    [SerializeField] private BounceType bounceType;
    [SerializeField] private float health;
    [SerializeField] private Vector3 Knockback;
    [SerializeField] private Hazards_HitStyle KnockbackApplication;
    [SerializeField] private int Strength;
    [SerializeField] private float Damage;
    public UnityEvent DamageEvent;
    private Quaternion HitRotation;
    public float Health()
    {
        return health;
    }

    public Vector3 Bounce()
    {
        return bounce;
    }

    public BounceType TypeBounce()
    {
        return bounceType;
    }

    public Transform HitTransform()
    {
        return transform;
    }

    public void DealDamage(float _damage, Vector3 _knockback, int _strength)
    {
        health -= _damage;

        if (health <= 0) DamageEvent.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable _damaged;
        Sonic_PlayerStateMachine _ctx;

        if (other.TryGetComponent(out _ctx) && other.TryGetComponent(out _damaged))
        {
            Debug.Log(_ctx.CurrentEstate);
            switch (KnockbackApplication)
            {
                case Hazards_HitStyle.Local:
                    HitRotation = transform.rotation;
                    _damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;

                case Hazards_HitStyle.World:
                    HitRotation = Quaternion.identity;
                    _damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;

                case Hazards_HitStyle.Circular:
                    HitRotation = Quaternion.FromToRotation(Vector3.forward,
                        Vector3.ProjectOnPlane(transform.position - _damaged.HitTransform().position, transform.up));
                    _damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;

                case Hazards_HitStyle.Spherical:
                    HitRotation = Quaternion.LookRotation(transform.position - _damaged.HitTransform().position);
                    _damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;
            }
        }
    }
}
