using UnityEngine;

public class GoalRing : CollectableBase
{
    public StageData Data;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            _ctx.MachineTransition(PlayerStates.Win);
            Data.Complete = true;
            RankCalc(_ctx);
        }
    }

    public int RankCalc(Sonic_PlayerStateMachine _ctx)
    {
        for (int i = 0; i < Data.RankScores.Count; i++)
        {
            if (_ctx.Chs.Rings >= Data.StageRingCount)
            {
                i = Data.RankScores.Count - 1;
                Debug.Log(i);
                continue;
            }
            if (_ctx.Chs.Score > Data.RankScores[i])
            {
                continue;
            }
            Debug.Log(i);
            return i;
        }
        return 0;
    }
}