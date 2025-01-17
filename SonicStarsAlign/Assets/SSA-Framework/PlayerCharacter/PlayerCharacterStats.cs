using System;
using UnityEngine;

[CreateAssetMenu]
public class PlayerCharacterStats : ScriptableObject
{
    [SerializeField]
    public float Rings;
    public float SetRings(float value)
    {
        Rings = value;
        RingSet?.Invoke(value);
        return Rings;
    }

    public event Action<float> RingSet;

    public IShield Shield;
}