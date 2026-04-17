using UnityEngine;
using UnityEngine.Events;

public class Hazards_HitBox : MonoBehaviour
{
    [SerializeField] private float Damage;
    private IDamageable Damaged;

    public UnityEvent HitEvent;
    private Quaternion HitRotation;

    [SerializeField] private Vector3 Knockback;
    [SerializeField] private Hazards_HitStyle KnockbackApplication;
    [SerializeField] private int Strength;
    [SerializeField] private bool PlayerOnly;

    private void OnTriggerEnter(Collider other)
    {
        if (isActiveAndEnabled && other.TryGetComponent(out Damaged))
        {
            if(PlayerOnly && !other.gameObject.CompareTag("Player")) return;
            
            switch (KnockbackApplication)
            {
                case Hazards_HitStyle.Local:
                    HitRotation = transform.rotation;
                    Damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;

                case Hazards_HitStyle.World:
                    HitRotation = Quaternion.identity;
                    Damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;

                case Hazards_HitStyle.Circular:
                    HitRotation = Quaternion.FromToRotation(Vector3.forward,
                        Vector3.ProjectOnPlane(transform.position - Damaged.HitTransform().position, transform.up));
                    Damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;

                case Hazards_HitStyle.Spherical:
                    HitRotation = Quaternion.LookRotation(transform.position - Damaged.HitTransform().position);
                    Damaged.DealDamage(Damage, HitRotation * Knockback, Strength);
                    break;
            }

            HitEvent.Invoke();
        }
    }
}