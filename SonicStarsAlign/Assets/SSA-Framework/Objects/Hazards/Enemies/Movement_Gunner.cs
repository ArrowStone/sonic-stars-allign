using System.Security.Cryptography;
using UnityEngine;

public class Movement_Gunner : Enemy_MovementStateMachine
{
    public Transform target;
    public LayerMask targetLayer;
    public LayerMask detectionBlockingMask;
    public float detectionDistance;
    public float dashDistance;
    private Overlap_Sphere detector;
    private Quaternion targetRotation;
    public float movementSpeed;
    public float rotationSpeed;
    private float shootTimer;
    private int shootCounter;
    [SerializeField] private float shootInterval;
    [SerializeField] private int shootCount;
    [SerializeField] private int shootDelay;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletVelocity;
    [SerializeField] private Transform BulletSpawn;
    [SerializeField] private AudioSource source;

    void Awake()
    {
        detector = new Overlap_Sphere(gameObject, 1, targetLayer, 0, detectionDistance, detectionBlockingMask, DetectionBias.Proximity);
        shootTimer = shootInterval;
        shootCounter = 0;
    }

    void Dash()
    {
        HorizontalVelocity = movementSpeed * transform.forward;
        Physics_ApplyVelocity();
    }

    new void FixedUpdate()
    {
        base.FixedUpdate();

        float _delta = Time.fixedDeltaTime;

        detector.Execute(transform.position, transform.forward);
        if(detector.TargetDetected) target = detector.TargetOutput.transform;
        // The sphere doesn't detect player if they're too close for some reason
        else if (target && Vector3.Distance(target.position, transform.position) > detectionDistance) target = null;

        if (target)
        {
            Vector3 targetLookPos = target.position ;
            targetLookPos.y = transform.position.y;
            targetRotation = Quaternion.LookRotation((targetLookPos - transform.position).normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _delta * rotationSpeed);
            shootTimer -= _delta;
            if(shootTimer <= 0)
            {
                shootTimer = shootInterval;

                Vector3 rotationVector = Vector3.ProjectOnPlane(target.position - BulletSpawn.position, Vector3.Cross(transform.forward, transform.up));

                if(Vector3.Angle(transform.forward, rotationVector) > 45) return;

                rotationVector += new Vector3(RandomNumberGenerator.GetInt32(-5, 5), RandomNumberGenerator.GetInt32(-5, 5), RandomNumberGenerator.GetInt32(-5, 5)) * 0.1f;
                Quaternion bulletRotation = Quaternion.LookRotation(rotationVector);
                bulletRotation.Normalize();

                GameObject bulletObj = Instantiate(bullet, BulletSpawn.position, bulletRotation);
                bulletObj.GetComponent<Rigidbody>().linearVelocity = bulletObj.transform.forward * bulletVelocity;

                if (shootCounter < shootCount) shootCounter += 1;
                else
                {
                    shootTimer = shootDelay;
                    shootCounter = 0;
                    if(Vector3.Distance(target.position, transform.position) < dashDistance) Dash();
                }

                source.Play();
            }
        }
        else
        {
            // Idling behaviour
            shootTimer = shootDelay;
        }
    }
}
