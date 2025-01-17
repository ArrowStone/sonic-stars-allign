using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Ring_Collection : CollectableBase
{
    public int Value;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            CollectionEvent.Invoke();
            _ctx.Chs.SetRings(_ctx.Chs.Rings + Value);
        }
    }
}