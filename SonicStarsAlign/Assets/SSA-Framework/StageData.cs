using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StageData : ScriptableObject
{
    public bool Complete;
    public List<bool> RedRings = new() { false, false, false, false, false };
    public List<int> RankScores = new() { 1000, 2000, 3000, 4000 };
    public int StageRingCount;
}