using UnityEngine;

public class Movement_Example : MonoBehaviour, Enemy_MovementBase
{
    private Rigidbody Rb;
    public Transform target;
    public LayerMask targetLayer;
    public LayerMask detectionBlockingMask;
    public float detectionDistance;
    private Overlap_Sphere detector;
    private Quaternion targetRotation;
    public float movementSpeed;
    public float rotationSpeed;

    void Awake()
    {
        detector = new Overlap_Sphere(gameObject, 1, targetLayer, 0, detectionDistance, detectionBlockingMask, DetectionBias.Proximity);
        Rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float _delta = Time.fixedDeltaTime;

        detector.Execute(transform.position, Vector3.forward);
        if(detector.TargetDetected) target = detector.TargetOutput.transform; else target = null;

        if (target)
        {
            Vector3 targetLookPos = target.position ;
            targetLookPos.y = transform.position.y;
            targetRotation = Quaternion.LookRotation((targetLookPos - transform.position).normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _delta * rotationSpeed);
            //Debug.Log();
            //Debug.DrawRay(transform.position, transform.forward * 10f, Color.red, _delta);
            Rb.linearVelocity = movementSpeed * Mathf.Max(0f, 1f - Quaternion.Angle(targetRotation, transform.rotation) * 0.02f) * transform.forward;
        }
        else
        {
            // Idling behaviour
        }
    }
}
