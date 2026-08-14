using UnityEngine;
using UnityEngine.Events;

// Responsible for triggering the results screen.
public class GoalRing : CollectableBase
{
    public StageData Data;
    public WinScreen winScreen;
    [SerializeField] private UnityEvent AdditionalReachEvent;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            _ctx.Snd.PlaySound("GoalRing");
            _ctx.MachineTransition(PlayerStates.Win);


            int rank = RankCalc((int)_ctx.Chs.Rings, _ctx.Chs.Score +
                                                        System.Math.Max(0, (int)(Data.TargetTime - _ctx.Chs.Time)) * 5);

            SceneSwitcher.Instance.CacheWinData(
                _ctx.Chs.Score,
                _ctx.Chs.Time,
                Data.TargetTime,
                _ctx.Chs.Rings,
                rank,
                Data.RedRings
            );

            // The trigger's name is meant to be the verb btw
            WinScreen.Instance.GetComponent<Animator>().SetTrigger("Present");
            AdditionalReachEvent.Invoke();
        }
    }

    public int RankCalc(int rings, int score)
    {
        Debug.Log("Rings: " + rings + ", Score: " + score);
        for (int i = 0; i < Data.RankScores.Length; i++)
        {
            Debug.Log(score + " vs " + Data.RankScores[i] + " for " + i);
            if (rings >= Data.StageRingCount)
            {
                i = Data.RankScores.Length - 1;
                continue;
            }
            if (score > Data.RankScores[i])
            {
                continue;
            }
            return i;
        }
        return Data.RankScores.Length;
    }
}