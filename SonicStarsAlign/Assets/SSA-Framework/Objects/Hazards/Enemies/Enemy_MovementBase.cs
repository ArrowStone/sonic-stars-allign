using UnityEngine;

// Common enemy code - mainly collision logic
public abstract class Enemy_MovementBase : MonoBehaviour
{
    private Rigidbody Rb;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public float Hover;
    public Vector3 Gravity;
    public float FallSpeedCap;
    public float DragCoefficient;
    public float groundRayLength;
    private Cast_Ray GroundCast;
    private Vector3 GroundNormal;
    private Vector3 Velocity;
    public Vector3 HorizontalVelocity;
    public Vector3 VerticalVelocity;

    void Start()
    {
        Rb = GetComponent<Rigidbody>();
        GroundCast = new Cast_Ray(groundRayLength, groundLayer);
        HorizontalVelocity = Vector3.zero;
        VerticalVelocity = Vector3.zero;
        Velocity = Vector3.zero;
    }

    public void GroundApplication ( float _delta ) {
        if(!GroundCast.Execute(transform.position, Gravity))
        {
            // Airborne
            Debug.Log(transform.name + " Airborne! " + VerticalVelocity.y);
            //Debug.Log(Gravity);
            HorizontalVelocity = HorizontalVelocity * DragCoefficient;
            VerticalVelocity += Gravity;
            if(VerticalVelocity.magnitude > FallSpeedCap)
                VerticalVelocity = VerticalVelocity.normalized * FallSpeedCap;
            Physics_ApplyVelocity();
            return;
        }
        GroundNormal = GroundCast.HitInfo.normal;
        HorizontalVelocity = Vector3.ProjectOnPlane(HorizontalVelocity, GroundNormal) * DragCoefficient;
        VerticalVelocity = Vector3.zero;
        Physics_ApplyVelocity();

        Vector3 targetPos = GroundCast.HitInfo.point + GroundNormal * Hover;
        Vector3 point0 = targetPos;
        Vector3 point1 = targetPos;
        point0 -= GroundNormal * 0.2f;
        point1 += GroundNormal * 0.2f;
        Collider[] colliders = new Collider[5];

        Physics_Snap(targetPos);
    }

    public void Physics_ApplyVelocity () {
        Velocity = HorizontalVelocity + VerticalVelocity;
        Rb.linearVelocity = Velocity;
    }

    public bool Physics_Sweep ( Vector3 _point, out RaycastHit Info ) {
        Vector3 _dif = _point - transform.position;
        return Rb.SweepTest(_dif, out Info, _dif.magnitude, QueryTriggerInteraction.Ignore) && Info.transform.gameObject.layer == wallLayer;
    }

    public void Physics_Snap ( Vector3 _point ) {
        if (!Physics_Sweep(_point, out _))
        {
            Player_StaticFunctions.MoveRBPosition(Rb, _point, "Physics Snap");
        }
    }
}