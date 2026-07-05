using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStats/Player Stats")]
public class PlayerCharacterStats : ScriptableObject
{
    // Idk why the rings are float so I won't touch it
    private float _rings;

    public float Rings
    {
        get { return _rings; }
        set { RingSet?.Invoke(value, _rings); _rings = value;}
    }

    private int _score;

    public int Score
    {
        get { return _score; }
        set { ScoreSet?.Invoke(value, _score); _score = value;}
    }

    private float _time;
    public float Time
    {
        get {return _time;}
        set {TimeSet?.Invoke(value, _time); _time = value;}
    }

    public event Action<float, float> RingSet;

    public event Action<float, float> ScoreSet;

    public event Action<float, float> TimeSet;

    public IShield Shield = null;

    public PosRot SpawnData;
}