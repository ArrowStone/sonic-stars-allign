using UnityEngine;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    public UnityEvent HitEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Sonic_PlayerStateMachine _ctx))
        {
            _ctx.Chs.SpawnData = new PosRot() { Position = transform.position + transform.rotation * offset, Rotation = transform.rotation };
            HitEvent.Invoke();
        }
    }
}