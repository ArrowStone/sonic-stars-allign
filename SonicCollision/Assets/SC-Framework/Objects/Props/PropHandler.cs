using UnityEngine;

// Handles props that Sonic surfs
public class PropHandler : MonoBehaviour
{
    [SerializeField] Vector3 UnmountDisplacement;
    [SerializeField] float PlayerSurfHover;
    private Collider DetectionCollider;

    void Start()
    {
        DetectionCollider = GetComponent<Collider>();
    }

    public PosRot Surf(Sonic_PlayerStateMachine _ctx)
    {
        PosRot _transfrm = new()
        {
            Position = _ctx.Rb.transform.position,
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        if (_ctx.SurfedProp) return _transfrm;
        DetectionCollider.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        _ctx.MachineTransition(PlayerStates.Surf);
        transform.parent = _ctx.transform;
        _ctx.SurfedProp = this;
        _ctx.SurfHover = _ctx.PlayerHover + PlayerSurfHover;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        return _transfrm;
    }

    public void Unmount(Sonic_PlayerStateMachine _ctx)
    {
        transform.parent = null;
        _ctx.SurfedProp = null;
        DetectionCollider.enabled = true;
        transform.Translate(UnmountDisplacement);
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
