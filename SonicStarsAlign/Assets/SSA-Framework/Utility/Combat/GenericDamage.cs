using UnityEngine;
using UnityEngine.Events;

public class GenericDamage : MonoBehaviour, IDamageable
{
    [SerializeField] private Vector3 _bounce;
    [SerializeField] private BounceType _bounceType;
    [SerializeField] private float IHealth;

    public float Strength;

    public UnityEvent DeathEvent;
    public UnityEvent HitEvent;

    public float Health()
    {
        return IHealth;
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

    public virtual void DealDamage(float _damage, Vector3 _knockback, int _strength)
    {
        if (_strength < Strength) return;
        IHealth -= _damage;
        if (IHealth <= 0)
        {
            DeathEvent.Invoke();
        }
        else
        {
            HitEvent.Invoke();
        }

        //Debug.Log(_damage);
    }
}