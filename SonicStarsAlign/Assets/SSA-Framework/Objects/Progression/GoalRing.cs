using UnityEngine;
using UnityEngine.Events;
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
            _ctx.Snd.PlaySound("GoalRing");
            CollectionEvent.Invoke();
            _ctx.MachineTransition(PlayerStates.Win);

            winScreen.goalRing = this;

            // The trigger's name is meant to be the verb btw
            WinScreen.Instance.GetComponent<Animator>().SetTrigger("Present");
            AdditionalReachEvent.Invoke();

            if(!(GameStateSaver && Data)) 
            {
                // Probably launched the scene in editor, don't bother
                Debug.LogWarning(string.Format( "Skipping saving progress due to absence of {0}{1}{2}.",
                                                GameStateSaver ? ""  : "GameStateSaver",
                                                !(GameStateSaver || Data) ? " and " : "",
                                                Data ? ""  : "Data"));
                return;
            }
            GameStateSaver.UpdateStageData(Data);
            Data.Complete = true;
            DataSaving.RecordStageData(Data);

            int rank = RankCalc(_ctx);
            SceneSwitcher.Instance.CacheWinData(
                _ctx.Chs.Score,
                GameStateSaver.hudManager.stageTimer,
                _ctx.Chs.Rings,
                rank
            );
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