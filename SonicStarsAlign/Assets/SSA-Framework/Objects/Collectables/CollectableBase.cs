using UnityEngine;
using UnityEngine.Events;

public abstract class CollectableBase : MonoBehaviour
{
    public float NoCollectionTime;
    public UnityEvent CollectionEvent;

    protected void FixedUpdate()
    {
        if (NoCollectionTime > 0) NoCollectionTime -= Time.fixedDeltaTime;
    }

    public virtual void OnTriggerEnter(Collider _trigger)
    {
        Debug.Log("Collected by: " + _trigger.gameObject.layer);
        if (NoCollectionTime > 0 ) return;
        if (_trigger.gameObject.layer != 11 && _trigger.transform.parent.gameObject.layer != 11) return;

        Collection(_trigger);
        CollectionEvent.Invoke();
    }

    public abstract void Collection(Collider _triggerer);
}