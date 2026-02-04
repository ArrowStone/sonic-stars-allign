using System;
using UnityEngine;

[CreateAssetMenu]
public class PlayerCharacterStats : ScriptableObject
{
    private float _rings;

    public float Rings
    {
        get { return _rings; }
        set { RingSet?.Invoke(value, _rings); _rings = value; Debug.Log(_rings);}
    }

    private float _score;

    public float Score
    {
        get { return _score; }
        set { ScoreSet?.Invoke(value, _score); _score = value; Debug.Log(_score); }
    }

    public event Action<float, float> RingSet;

    public event Action<float, float> ScoreSet;

    public IShield Shield = null;

    public PosRot SpawnData;
}