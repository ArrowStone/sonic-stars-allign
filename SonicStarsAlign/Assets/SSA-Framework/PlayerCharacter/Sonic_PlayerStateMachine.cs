using System;
using UnityEngine;

public class Sonic_PlayerStateMachine : StateMachine_MonoBase<PlayerStates>
{
    public InputComponent Input;
    public Transform InputRef;
    public Rigidbody Rb;
    public CapsuleCollider Cl;
    public PlayerCharacterStats Chs;
    public float InvinciblitiyState;
    public bool TrickState;

    [Header("Handling")]
    public AnimationCurve ChrTurn;

    public float SnapForce;

    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private LayerMask wallLayer;

    [Space]
    [SerializeField] private float groundRayDig;

    [SerializeField] private float groundRayLength;

    [Space]
    [SerializeField] private Vector3 normCollCenter;

    [SerializeField] private float normCollHeight;

    [Space]
    [SerializeField] private Vector3 crouchCollCenter;

    [SerializeField] private float crouchCollHeight;

    #region Util

    public float PlayerHover { get => groundRayLength - groundRayDig; }
    public Vector3 PlayerDirection { get; set; } = Vector3.forward;
    public Vector3 InputVector { get; set; }
    public Quaternion InputRotation { get; set; }
    public Vector3 Gravity { get; set; } = Vector3.down;
    public Vector3 GroundNormal { get; set; } = Vector3.up;
    public Vector3 HorizontalVelocity { get; set; } = Vector3.zero;
    public Vector3 VerticalVelocity { get; set; } = Vector3.zero;
    public Collider TriggerBuffer { get; set; }

    public Vector3 Velocity
    {
        get => Rb.linearVelocity;
        set
        {
            if (Rb.isKinematic)
            {
                return;
            }

            Rb.linearVelocity = value;
        }
    }

    public Cast_Ray GroundCast { get; private set; }
    public Cast_Ray WallCast { get; private set; }
    public Cast_Ray CeilCast { get; private set; }

    public event Action JumpAction;

    #endregion Util

    #region Moves

    private bool _jumping;

    public bool Jumping
    {
        get => _jumping;
        set
        {
            _jumping = value;
            if (value)
            {
                JumpAction?.Invoke();
            }
        }
    }

    public int AirBoosts { get; set; }
    public bool Death { get; set; }
    public bool Skid { get; set; }

    #endregion Moves

    public void ComponentSetup()
    {
        GroundCast = new Cast_Ray(groundRayLength, groundLayer);
        WallCast = new Cast_Ray(groundRayLength, wallLayer);
        CeilCast = new Cast_Ray(groundRayLength, wallLayer);
    }

    public void StateSetup()
    {
        Sonic_AirState cashedAirState = new(this);
        States.Add(PlayerStates.Ground, new Sonic_GroundState(this));
        States.Add(PlayerStates.Air, new Sonic_AirState(this));
        States.Add(PlayerStates.Spindash, cashedAirState);
        States.Add(PlayerStates.Roll, cashedAirState);

        CurrentEstate = PlayerStates.Air;
        CurrentState = States[CurrentEstate];
        CurrentState.EnterState();
    }

    public void Start()
    {
        ComponentSetup();
        StateSetup();
        Initialize();
    }

    public void Update()
    {
        base.MachineUpdate();
    }

    public void FixedUpdate()
    {
        base.MachineFixedUpdate();
    }

    #region AdditionalFunctions

    public void Physics_ApplyVelocity()
    {
        Velocity = HorizontalVelocity + VerticalVelocity;
    }

    private Quaternion cashedRotation = Quaternion.identity;

    public Quaternion Physics_Rotate(Vector3 _forward, Vector3 _up)
    {
        Quaternion _diff = Quaternion.LookRotation(_forward, cashedRotation * Vector3.up);
        _diff = Quaternion.FromToRotation(_diff * Vector3.up, _up.normalized) * _diff;
        _diff *= Quaternion.Inverse(cashedRotation);

        Rb.MoveRotation(_diff * cashedRotation);
        cashedRotation = Rb.rotation;
        return _diff;
    }

    public bool Physics_Sweep(Vector3 _point, out RaycastHit Info)
    {
        Vector3 _dif = _point - Rb.position;
        return Rb.SweepTest(_dif, out Info, _dif.magnitude, QueryTriggerInteraction.Ignore);
    }

    public void Physics_Snap(Vector3 _point)
    {
        if (!Physics_Sweep(_point, out _))
        {
            Rb.position = _point;
        }
    }

    public void Physics_Snap(Vector3 _point, float _time)
    {
        Vector3 _v = Vector3.Lerp(Rb.position, _point, _time);
        if (!Physics_Sweep(_v, out _))
        {
            Rb.position = _v;
        }
    }

    #region Debug

#if UNITY_EDITOR

    public void OnDrawGizmos()
    {
        Debug.DrawLine(Rb.worldCenterOfMass, Rb.worldCenterOfMass + (InputRotation * Vector3.up * 1), Color.yellow);

        if (GroundCast == null)
        {
            return;
        }

        if (CurrentEstate is PlayerStates.Ground)
        {
            Debug.DrawLine(Rb.worldCenterOfMass, GroundCast.HitInfo.point, Color.green);
        }
        else
        {
            if (Vector3.Dot(Velocity, Gravity.normalized) > 0)
            {
                Debug.DrawLine(Rb.worldCenterOfMass, Rb.worldCenterOfMass + (Gravity * groundRayLength), Color.red);
            }
            else
            {
                Debug.DrawLine(Rb.worldCenterOfMass, Rb.worldCenterOfMass + (-Gravity * groundRayLength), Color.red);
            }
        }
    }

#endif

    #endregion Debug

    #region Jump

    public void Jump(float _delta)
    {
        Jumping = true;
        VerticalVelocity += (GroundNormal * Chs.JumpForce) - (Gravity * (Chs.GravityForce * _delta));

        Physics_ApplyVelocity();
        MachineTransition(PlayerStates.Air);
    }

    #endregion Jump

    #endregion AdditionalFunctions
}

public enum PlayerStates
{
    Ground,
    Air,
    Stomp,
    HopJump,
    HummingTop,
    HommingAttack,
    LightSpeedDash,
    Roll,
    Spindash,
    Damage,
    RailGrinding,
    LinearAutomation,
}