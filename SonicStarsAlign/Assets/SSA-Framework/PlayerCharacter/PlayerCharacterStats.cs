using System;
using UnityEngine;

[CreateAssetMenu]
public class PlayerCharacterStats : ScriptableObject
{
    private float _rings;

    public float Rings
    {
        get { return _rings; }
        set { _rings = value; Debug.Log(_rings); RingSet?.Invoke(value); }
    }

    private float _score;

    public float Score
    {
        get { return _score; }
        set { _score = value; Debug.Log(_score); ScoreSet?.Invoke(value); }
    }

    public event Action<float> RingSet;

    public event Action<float> ScoreSet;

    public IShield Shield = null;

    public PosRot SpawnData;
}