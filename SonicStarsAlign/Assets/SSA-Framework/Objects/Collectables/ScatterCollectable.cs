using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ScatterCollectable : MonoBehaviour
{
    [SerializeField] private float castDist;
    [SerializeField] private float gravity;
    [SerializeField] private float bounce;

    [Space]
    [SerializeField] private LayerMask layerMask;

    [Space]
    [SerializeField] private bool despawn;
    [SerializeField] private float despawnTime;

    public UnityEvent DespawnEvent;

    #region  Util
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
        GravityCalculations();
        GroundCheck();
    }

    private void Update()
    {
        if (!despawn)
            return;

        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            DespawnEvent.Invoke();
        }
    }

    private void GravityCalculations()
    {
        Rb.linearVelocity += GravityDirection * gravity;
    }

    private void GroundCheck()
    {
        if (groundDetector.Execute(transform.position, Rb.linearVelocity.normalized))
            Rb.linearVelocity = Vector3.ProjectOnPlane(Rb.linearVelocity, groundDetector.HitInfo.normal) +
                          Vector3.Dot(Rb.linearVelocity, -groundDetector.HitInfo.normal) * bounce *
                          groundDetector.HitInfo.normal;
    }

    public void End()
    {
        Destroy(gameObject);
    }
}