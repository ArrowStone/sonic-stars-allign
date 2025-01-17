using UnityEngine;

public interface IDamageable
{
    public float Health();

    public Vector3 Bounce();

    public BounceType TypeBounce();

    public Transform HitTransform();

    public void DealDamage(float _damage, Vector3 _knockback, int _strength);
}

public enum BounceType
{
    BounceLocal,
    BounceWorld,
    LockLocal,
    LockWorld
}