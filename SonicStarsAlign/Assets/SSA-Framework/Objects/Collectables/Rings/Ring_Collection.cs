using UnityEngine;

public class Ring_Collection : CollectableBase
{
    public float EnergyValue;
    public int RingValue;

    public override void Collection(Collider _triggerer)
    {
        if (_triggerer.transform.TryGetComponent(out Sonic_PlayerStateMachine Collector))
        {
            CollectionEvent.Invoke();
          //  Collector.Stats.ChangeRings(RingValue);
           // Collector.Stats.ChangeBoost(EnergyValue);
        }
    }
}