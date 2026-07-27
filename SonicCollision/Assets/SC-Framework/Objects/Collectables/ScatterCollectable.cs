using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ScatterCollectable : MonoBehaviour
{
    [SerializeField] private float castDist;
    [SerializeField] private float gravity;
    [SerializeField] private float frictionCoefficient;
    [SerializeField] private float bounce;
    [SerializeField] private float minimumBounce;

    [Space]
    [SerializeField] private LayerMask layerMask;

    [Space]
    [SerializeField] private bool despawn;

    [SerializeField] private float despawnTime;

    public UnityEvent DespawnEvent;

    #region Util

    private Cast_Ray groundDetector;
    private float time;
    private Rigidbody Rb;
    public Vector3 GravityDirection { get; set; }

    #endregion Util

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        GravityDirection = new Vector3(0, -1, 0);
        groundDetector = new Cast_Ray(castDist, layerMask);
        time = despawnTime;
    }

    public void SetGravity(Vector3 _gravity)
    {
        GravityDirection = _gravity;
    }

    private void FixedUpdate()
    {
        float _delta = Time.fixedDeltaTime;

        GravityCalculations();
        GroundCheck(_delta);

        if (!despawn)
            return;

        if (time > 0)
        {
            time -= _delta;
        }
        else
        {
            DespawnEvent.Invoke();
        }
    }

    private void Update()
    {

    }

    private void GravityCalculations()
    {
        Rb.linearVelocity += GravityDirection * gravity;
    }

    private void GroundCheck(float _delta)
    {
        if (groundDetector.Execute(transform.position, Rb.linearVelocity.normalized))
        {
            Vector3 bounceVelocity = Vector3.Dot(Rb.linearVelocity, -groundDetector.HitInfo.normal) * bounce * groundDetector.HitInfo.normal;

            if (bounceVelocity.magnitude < minimumBounce)
            {
                bounceVelocity = Vector3.zero;
            }

            Rb.linearVelocity = Vector3.ProjectOnPlane(Rb.linearVelocity, groundDetector.HitInfo.normal) * frictionCoefficient + bounceVelocity;
        }
    }

    public void End()
    {
        Destroy(gameObject);
    }
}