using UnityEngine;

// Enemy movement & collision
public abstract class Enemy_MovementStateMachine : StateMachine_MonoBase<EnemyStates>
{
    private Rigidbody Rb;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public float Hover;
    public Vector3 Gravity;
    public float FallSpeedCap;
    public float DragCoefficient;
    public float groundRayLength;
    public Cast_Ray GroundCast;
    public Vector3 GroundNormal;
    private Vector3 Velocity;
    public Vector3 HorizontalVelocity;
    public Vector3 VerticalVelocity;

    public void Start()
    {
        Rb = GetComponent<Rigidbody>();
        GroundCast = new Cast_Ray(groundRayLength, groundLayer);
        HorizontalVelocity = Vector3.zero;
        VerticalVelocity = Vector3.zero;
        Velocity = Vector3.zero;

        Initialize();

        States.Add(EnemyStates.Ground, new Enemy_GroundState(this));
        States.Add(EnemyStates.Air, new Enemy_AirState(this));

        CurrentEstate = EnemyStates.Air;
        CurrentState = States[CurrentEstate];
        CurrentState.EnterState();
    }

    public void Update()
    {
        base.MachineUpdate();
    }

    public void FixedUpdate()
    {
        //Debug.Log(CurrentEstate);
        base.MachineFixedUpdate();
    }

    public void LateUpdate ()
    {
        base.MachineLateUpdate();
    }

    public void Physics_ApplyVelocity () 
    {
        Velocity = HorizontalVelocity + VerticalVelocity;
        Rb.linearVelocity = Velocity;
    }

    public bool Physics_Sweep ( Vector3 _point, out RaycastHit Info ) 
    {
        Vector3 _dif = _point - transform.position;
        return Rb.SweepTest(_dif, out Info, _dif.magnitude, QueryTriggerInteraction.Ignore) && Info.transform.gameObject.layer == wallLayer;
    }

    public void Physics_Snap ( Vector3 _point ) 
    {
        if (!Physics_Sweep(_point, out _))
        {
            Player_StaticFunctions.MoveRBPosition(Rb, _point, "Physics Snap");
        }
    }
}

public enum EnemyStates
{
    Ground,
    Air
}