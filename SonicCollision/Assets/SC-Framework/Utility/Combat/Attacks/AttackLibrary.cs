using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AttackLibrary
{
    public GameObject AttackObject;
    public LayerMask AttackMask;
    public List<Attack> AttacksLibrary = new();

    public void Initialize()
    {
        foreach (Attack _attack in AttacksLibrary)
        {
            _attack.Setup(AttackObject.transform, AttackMask);
        }
    }

    public void AttackUpdate(float _delta = 0.02f)
    {
        foreach (Attack _attack in AttacksLibrary.Where(h => h != null && h.Active))
        {
            _attack.ExecuteAttack();
            if (_attack.RefreshRate > 0)
            {
                _attack.RefreshCalculations(_delta);
            }
        }
    }

    public Attack GetAttack(string _attack)
    {
        return AttacksLibrary.Find(attack => attack.Name == _attack);
    }

    public int AttackStrength()
    {
        int i = 0;
        foreach (Attack a in AttacksLibrary)
        {
            if (a.Strength > i)
                i = a.Strength;
        }
        return i;
    }

    public bool Active()
    {
        return AttacksLibrary.Any(attack => attack.Active);
    }

    public void StartAttack(string _attack)
    {
        AttacksLibrary.Find(attack => attack.Name == _attack).Active = true;
    }

    public void StopAttack(string _attack)
    {
        Attack a = AttacksLibrary.Find(attack => attack.Name == _attack);
        if (a.Active == false)
        {
            return;
        }

        a.Active = false;
        a.Clear();
    }

    public void StopAll()
    {
        foreach (Attack _attack in AttacksLibrary)
        {
            _attack.Active = false;
            _attack.Clear();
        }
    }
}