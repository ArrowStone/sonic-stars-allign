using System;
using UnityEngine;

// Obtains the required data to be saved as well as setting the saved values when loading.
// Also handles stage resetting
public class GameStateManager : MonoBehaviour
{
    public StageData[] StageDataAssets;
    public Sonic_PlayerStateMachine ctx;
    private ResetHandler[] ResetHandlerArray;
    private SpawnPoint spawnPoint;
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
        ResetHandlerArray = FindObjectsByType<ResetHandler>();
        spawnPoint = FindAnyObjectByType<SpawnPoint>();
    }

    public void ResetStage()
    {
        ctx.Chs.Rings = 0;
        ctx.Chs.Score = 0;
        ctx.Chs.Time = 0;
        foreach (ResetHandler handler in ResetHandlerArray)
        {
            handler.Reset();
        }
        spawnPoint.SetPos();
        ctx.MachineTransition(PlayerStates.Air);
        ctx.HorizontalVelocity = Vector3.zero;
        ctx.VerticalVelocity = Vector3.zero;
        ctx.Physics_ApplyVelocity();
    }

    public void RecordCheckpoint()
    {
        foreach (ResetHandler handler in ResetHandlerArray)
        {
            handler.RecordCheckpoint();
        }
    }

    public void ResetToCheckpoint()
    {
        foreach (ResetHandler handler in ResetHandlerArray)
        {
            handler.ResetToCheckpoint();
        }
    }
}
