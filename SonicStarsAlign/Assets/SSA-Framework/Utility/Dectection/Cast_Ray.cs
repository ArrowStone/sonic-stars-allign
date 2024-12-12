using UnityEngine;

public class Cast_Ray
{
    public float DetectionDistance;

    private readonly LayerMask targetMask;
    private readonly QueryTriggerInteraction triggerInteraction;

    public RaycastHit HitInfo;

    public Cast_Ray(float _distance, LayerMask _layerMask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore)
    {
        DetectionDistance = _distance;
        targetMask = _layerMask;
        triggerInteraction = _triggerInteraction;
    }

    public bool Execute(Vector3 _position, Vector3 _direction)
    {
        _direction.Normalize();
        return Physics.Raycast(_position, _direction, out HitInfo, DetectionDistance, targetMask, triggerInteraction);
    }
}