using System.Security.Cryptography;
using UnityEngine;

public class Movement_Gunner : MonoBehaviour, Enemy_MovementBase
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
        Rb = GetComponent<Rigidbody>();
        shootTimer = shootInterval;
        shootCounter = 0;
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
            //Rb.linearVelocity = movementSpeed * Mathf.Max(0f, 1f - Quaternion.Angle(targetRotation, transform.rotation) * 0.02f) * transform.forward;
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
