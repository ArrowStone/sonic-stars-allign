using UnityEngine;

public class Cast_Sphere
{
    public float DetectionDistance;
    public float DetectionRadius;

    private readonly LayerMask targetMask;
    private readonly QueryTriggerInteraction triggerInteraction;

    public RaycastHit HitInfo;

    public Cast_Sphere(float _distance, float _radius, LayerMask _layerMask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore)
    {
        DetectionDistance = _distance;
        DetectionRadius = _radius;
        targetMask = _layerMask;
        triggerInteraction = _triggerInteraction;
    }

    public bool Execute(Vector3 _position, Vector3 _direction)
    {
        _direction.Normalize();
        return Physics.SphereCast(_position, DetectionRadius, _direction, out HitInfo, DetectionDistance, targetMask, triggerInteraction);
    }
}