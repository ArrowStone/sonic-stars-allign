using UnityEngine;
using UnityEngine.Events;

// Responsible for triggering stat saving and the results screen.
public class GoalRing : CollectableBase
{
    public StageData Data;
    public GameStateManager GameStateSaver;
    public WinScreen winScreen;
    [SerializeField] private UnityEvent AdditionalReachEvent;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            _ctx.Snd.PlaySound("GoalRing");
            _ctx.MachineTransition(PlayerStates.Win);


            int rank = RankCalc((int) _ctx.Chs.Rings,   _ctx.Chs.Score + 
                                                        System.Math.Max(0, (int) (Data.TargetTime - _ctx.Chs.Time)) * 5);

            if(!(GameStateSaver && Data)) 
            {
                // Probably launched the scene in editor, don't bother
                Debug.LogWarning(string.Format( "Skipping saving progress due to absence of {0}{1}{2}.",
                                                GameStateSaver ? ""  : "GameStateSaver",
                                                !(GameStateSaver || Data) ? " and " : "", // I'm so clever
                                                Data ? ""  : "Data"));
            }
            else
            {
                GameStateSaver.UpdateStageData(Data, (byte) rank);
                Data.Complete = true;
                DataSaving.RecordStageData(Data);
            }

            SceneSwitcher.Instance.CacheWinData(
                _ctx.Chs.Score,
                _ctx.Chs.Time,
                Data.TargetTime,
                _ctx.Chs.Rings,
                rank
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