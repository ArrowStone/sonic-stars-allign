using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class StageData : ScriptableObject
{
    public string saveFile;
    public bool Complete = false;
    public List<bool> RedRings = new() { false, false, false, false, false };
    public List<int> RankScores = new() { 1000, 2000, 3000, 4000 };
    public int StageRingCount;
}