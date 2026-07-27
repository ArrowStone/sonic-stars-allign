using UnityEngine;

public class Cast_Box
{
    public float DetectionDistance;
    public Vector3 extents;

    private readonly LayerMask targetMask;
    private readonly QueryTriggerInteraction triggerInteraction;

    public RaycastHit HitInfo;

    public Cast_Box(float _dist, Vector3 _extents, LayerMask _target)
    {
        DetectionDistance = _dist;
        extents = _extents;
        targetMask = _target;
    }

    public bool Execute(Vector3 _orign, Vector3 _direction, Quaternion _orientation)
    {
        return Physics.BoxCast(_orign, extents, _direction, out HitInfo, _orientation, DetectionDistance, targetMask);
    }
}