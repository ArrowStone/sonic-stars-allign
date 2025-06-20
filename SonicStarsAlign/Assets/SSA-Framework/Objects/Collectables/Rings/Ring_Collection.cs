using UnityEngine;

public class Ring_Collection : CollectableBase
{
    public int Value;
    public int ScoreValue;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            _ctx.Chs.Rings += Value;
            _ctx.Chs.Score += ScoreValue;
        }
    }
}