using UnityEngine;

public class RedRing_Collection : CollectableBase
{
    public StageData CurStageData;

    public int Place;
    public int ScoreValue;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            CurStageData.RedRings[Place] = true;
            _ctx.Chs.Score += ScoreValue;
        }
    }
}