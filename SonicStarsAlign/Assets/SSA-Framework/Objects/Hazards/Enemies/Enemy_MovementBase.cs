using UnityEngine;

// Common enemy code - mainly collision logic
public abstract class Enemy_MovementBase : MonoBehaviour
{
    private Rigidbody Rb;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public float Hover;
    public float groundRayLength;
    private Cast_Ray GroundCast;
    private Vector3 GroundNormal;
    private Vector3 Velocity;
    public Vector3 HorizontalVelocity;
    public Vector3 VerticalVelocity;
    private Vector3 prevPos;

    void Start()
    {
        Rb = GetComponent<Rigidbody>();
        GroundCast = new Cast_Ray(groundRayLength, groundLayer);
        HorizontalVelocity = Vector3.zero;
        VerticalVelocity = Vector3.zero;
        Velocity = Vector3.zero;
        prevPos = transform.position;
    }

    public void GroundApplication ( float _delta ) {
        if(!GroundCast.Execute(transform.position, -Vector3.up))
        {
            transform.position = prevPos;
            HorizontalVelocity = Vector3.zero;
            VerticalVelocity = Vector3.zero;
            Physics_ApplyVelocity();
            return;
        }
        GroundNormal = GroundCast.HitInfo.normal;
        HorizontalVelocity = Vector3.ProjectOnPlane(Velocity, GroundNormal).normalized * Velocity.magnitude;

        Vector3 targetPos = GroundCast.HitInfo.point + GroundNormal * Hover;
        Vector3 point0 = targetPos;
        Vector3 point1 = targetPos;
        point0 -= GroundNormal * 0.2f;
        point1 += GroundNormal * 0.2f;
        Collider[] colliders = new Collider[5];

        // Simple point check isn't enough, player can sometimes clip
        if (Physics.OverlapCapsuleNonAlloc(point0, point1, 0.25f, colliders, wallLayer) < 2f) Physics_Snap(targetPos);
        //Physics_Snap(targetPos);
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
            prevPos = transform.position;
            Player_StaticFunctions.MoveRBPosition(Rb, _point, "Physics Snap");
        }
    }
}