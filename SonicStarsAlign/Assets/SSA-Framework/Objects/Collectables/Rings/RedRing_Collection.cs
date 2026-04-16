using UnityEngine;
using UnityEngine.Events;

public class RedRing_Collection : CollectableBase
{
    public StageData CurStageData;
    public int Place;
    public int ScoreValue;
    public UnityEvent OnAllRedRingsCollected;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            CurStageData.RedRings[Place] = true;
            _ctx.Chs.Score += ScoreValue;

            if (AllRedRingsCollected())
            {
                Debug.Log("All red rings collected!");
                DataSaving.RecordStageData(CurStageData); // saves to disk
                OnAllRedRingsCollected.Invoke();
            }
        }
    }

    private bool AllRedRingsCollected()
    {
        foreach (bool ring in CurStageData.RedRings)
            if (!ring) return false;
        return true;
    }
}