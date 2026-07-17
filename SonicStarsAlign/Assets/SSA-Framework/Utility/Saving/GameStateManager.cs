using System;
using UnityEngine;

// Obtains the required data to be saved as well as setting the saved values when loading.
public class GameStateManager : MonoBehaviour
{
    public StageData[] StageDataAssets;
    public Sonic_PlayerStateMachine ctx;
    public void LoadData()
    {
        if (StageDataAssets == null) return;

        for (int i = 0; i < StageDataAssets.Length; i++)
        {
            DataSaving.ReadStageData(StageDataAssets[i]);
        }
    }

    public void UpdateStageData(StageData data, byte rank)
    {
        if (!data.Complete)
        {
            data.bestTime = ctx.Chs.Time;
            data.bestScore = ctx.Chs.Score;
            data.Rank = rank;
        }
        data.bestTime = Math.Min(data.bestTime, ctx.Chs.Time);
        data.bestScore = Math.Max(data.bestScore, ctx.Chs.Score);
        data.Rank = Math.Max(data.Rank, rank);
    }

    void Awake()
    {
        LoadData();
    }
}
