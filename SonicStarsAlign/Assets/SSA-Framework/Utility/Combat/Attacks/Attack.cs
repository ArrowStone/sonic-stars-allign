using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Attack
{
    public bool Active { get; set; }
    private readonly List<IDamageable> _attacked = new();
    private IDamageable damageableComponent;
    private LayerMask _aLayerMask;
    private Transform _aTransform;
    private Collider[] Hits = new Collider[0];
    private int TargetCount;

    public void Setup(Transform _source, LayerMask _attackLayerMask)
    {
        _aTransform = _source;
        _aLayerMask = _attackLayerMask;
        curRefreshRate = RefreshRate;
    }

    public void Clear()
    {
        if (_attacked.Count > 0)
        {
            _attacked.Clear();
        }
    }

    public void ExecuteAttack()
    {
        Hits = new Collider[maxHitsCount];
        TargetCount = Physics.OverlapSphereNonAlloc(_aTransform.position + (_aTransform.rotation * Offset), radius, Hits, _aLayerMask);

        if (TargetCount < 1)
        {
            Clear();
            return;
        }

        foreach (Collider hit in Hits.Where(h => h != null && h.TryGetComponent(out damageableComponent)))
        {
            if (_attacked.Contains(damageableComponent))
            {
                continue;
            }

            DealDamage(damageableComponent);
        }
    }

    private void DealDamage(IDamageable _damageable)
    {
        HitAction?.Invoke(_damageable);
        _attacked.Add(_damageable);

        switch (knockbackApplication)
        {
            case Hazards_HitStyle.Local:
                hitRotation = _aTransform.rotation;
                _damageable.DealDamage(Damage, hitRotation * knockback, Strength);
                break;

            case Hazards_HitStyle.World:
                hitRotation = Quaternion.identity;
                _damageable.DealDamage(Damage, hitRotation * knockback, Strength);
                break;

            case Hazards_HitStyle.Circular:
                hitRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(_aTransform.position - _damageable.HitTransform().position, _aTransform.up));
                _damageable.DealDamage(Damage, hitRotation * knockback, Strength);
                break;

            case Hazards_HitStyle.Spherical:
                hitRotation = Quaternion.LookRotation(_aTransform.position - _damageable.HitTransform().position);
                _damageable.DealDamage(Damage, hitRotation * knockback, Strength);
                break;
        }
    }

    public void RefreshCalculations(float _timeForm)
    {
        curRefreshRate -= _timeForm;
        if (curRefreshRate <= 0)
        {
            curRefreshRate = RefreshRate;
            _attacked.Clear();
        }
    }

    #region AttackProperties

    public string Name;
    [SerializeField] private int maxHitsCount;
    [Space] public float radius;
    public Vector3 Offset;
    [Space][SerializeField] private Vector3 knockback;
    [SerializeField] private Hazards_HitStyle knockbackApplication;
    [Space] public float Damage;
    public int Strength;
    public float RefreshRate;
    private float curRefreshRate;
    private Quaternion hitRotation;

    public event Action<IDamageable> HitAction;

    #endregion AttackProperties
}

public enum Hazards_HitStyle
{
    Spherical,
    Circular,
    Local,
    World
}