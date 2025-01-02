using UnityEngine;
using UnityEngine.Events;

public abstract class CollectableBase : MonoBehaviour
{
    public float NoCollectionTime;
    public UnityEvent CollectionEvent;

    protected void Update()
    {
        if (NoCollectionTime > 0) NoCollectionTime -= Time.deltaTime;
    }

    public virtual void OnTriggerEnter(Collider _trigger)
    {
        if (NoCollectionTime > 0) return;

        CollectionEvent.Invoke();
        Collection(_trigger);
    }

    public abstract void Collection(Collider _triggerer);
}