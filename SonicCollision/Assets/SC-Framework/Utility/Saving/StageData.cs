using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class StageData : ScriptableObject
{
    public string saveFile;
    public bool Complete = false;
    public byte Rank = 0;
    public bool[] RedRings = { false, false, false, false, false };
    public bool CollectedAllRedrings = false; // Not changed by collecting red rings to check if they ahve been collected in a precious attempt
    public float bestTime = 0;
    public int bestScore = 0;
    public int[] RankScores = { 1000, 2000, 3000, 4000 };
    public int StageRingCount;
    public float TargetTime;
}