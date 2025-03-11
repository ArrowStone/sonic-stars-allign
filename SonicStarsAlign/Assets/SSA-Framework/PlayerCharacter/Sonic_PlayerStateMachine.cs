using System;
using UnityEngine;

public class Sonic_PlayerStateMachine : StateMachine_MonoBase<PlayerStates>
{
    public InputComponent Input;
    public Transform InputRef;
    public Rigidbody Rb;
    public CapsuleCollider Cl;
    public Panel_Collider TriggerCl;
    public PlayerCharacterParameters Chp;
    public PlayerCharacterStats Chs;
    public float InvinciblitiyState;
    public bool TrickState;

    [Header("Handling")]
    public AnimationCurve ChrTurn;

    public float SnapForce;

    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private LayerMask homingTargetLayer;

    [SerializeField] private LayerMask ringLayer;

    [Space]
    [SerializeField] private float homingDetectionDistance;

    [SerializeField] private float homingDetectionRadius;

    [SerializeField] private float lightDetectionDistance;

    [SerializeField] private float lightDetectionRadius;

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

    public float PlayerHover => groundRayLength - groundRayDig;
    public Vector3 PlayerDirection { get; set; } = Vector3.forward;
    public Vector3 InputVector { get; set; }
    public Quaternion InputRotation { get; set; }
    public Vector3 Gravity { get; set; } = Vector3.down;
    public Vector3 GroundNormal { get; set; } = Vector3.up;
    public Vector3 HorizontalVelocity { get; set; } = Vector3.zero;
    public Vector3 VerticalVelocity { get; set; } = Vector3.zero;
    public Collider TriggerBuffer { get; set; }
    public Vector3 CurrentMoveDirection { get; set; }

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

    public SplineHandler SplnHandler { get; set; }
    public Cast_Ray GroundCast { get; private set; }
    public Cast_Ray WallCast { get; private set; }
    public Cast_Ray CeilCast { get; private set; }
    public Overlap_Sphere RingDetector { get; private set; }
    public Overlap_Sphere HomingTargetDetector { get; private set; }

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

    private bool _dropDashing;

    public bool DropDashing
    {
        get => _dropDashing;
        set
        {
            _dropDashing = value;
            if (value)
            {
                DropAction?.Invoke();
            }
        }
    }

    public event Action JumpAction;

    public int AirDashes = 1;

    public event Action DashAction;

    public event Action DropAction;

    public int BounceCount { get; set; }
    public int AirBoosts { get; set; }
    public bool Death { get; set; }
    public bool Skid { get; set; }

    #endregion Moves

    public void ComponentSetup()
    {
        GroundCast = new Cast_Ray(groundRayLength, groundLayer);
        WallCast = new Cast_Ray(groundRayLength, wallLayer);
        CeilCast = new Cast_Ray(groundRayLength, wallLayer);
        HomingTargetDetector = new Overlap_Sphere(gameObject, 5, homingTargetLayer, homingDetectionDistance, homingDetectionRadius, wallLayer, DetectionBias.Direction);
        RingDetector = new Overlap_Sphere(gameObject, 5, ringLayer, lightDetectionDistance, lightDetectionRadius, wallLayer, DetectionBias.Direction);
        SplnHandler = new SplineHandler();
    }

    public void StateSetup()
    {
        States.Add(PlayerStates.Ground, new Sonic_GroundState(this));
        States.Add(PlayerStates.Air, new Sonic_AirState(this));
        States.Add(PlayerStates.Spindash, new Sonic_SpinDashState(this));
        States.Add(PlayerStates.Roll, new Sonic_RollState(this));
        States.Add(PlayerStates.Bounce, new Sonic_BounceState(this));
        States.Add(PlayerStates.HomingAttack, new Sonic_HomingAttackState(this));
        States.Add(PlayerStates.LightSpeedDash, new Sonic_LightDashState(this));
        States.Add(PlayerStates.Damage, new Sonic_DamageState(this));
        States.Add(PlayerStates.RailGrinding, new Sonic_GrindState(this));
        States.Add(PlayerStates.LinearAutomation, new Sonic_LinearAutomationState(this));
        States.Add(PlayerStates.Pully, new Sonic_PullyState(this));
        States.Add(PlayerStates.Pole, new Sonic_PoleState(this));
        States.Add(PlayerStates.RailSwitch, new Sonic_RailSwitchState(this));
        States.Add(PlayerStates.Win, new Sonic_WinState(this));

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
        Chs.Shield?.Execute(Time.deltaTime);
    }

    public void FixedUpdate()
    {
        base.MachineFixedUpdate();
    }

    public void LateUpdate()
    {
        base.MachineLateUpdate();
    }

    #region AdditionalFunctions
    public void Physics_ApplyVelocity()
    {
        Velocity = HorizontalVelocity + VerticalVelocity;
    }

    public void Respawn()
    {
        transform.SetPositionAndRotation(Chs.SpawnData.Position, Chs.SpawnData.Rotation);
        MachineTransition(PlayerStates.Ground);
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
        return Rb.SweepTest(_dif, out Info, _dif.magnitude, QueryTriggerInteraction.Ignore) && Info.transform.gameObject.layer == wallLayer;
    }

    public void Physics_Snap(Vector3 _point)
    {
        if (!Physics_Sweep(_point, out _))
        {
            Rb.MovePosition(_point);
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

    public void ChangeKinematic(bool _t)
    {
        Cl.enabled = false;
        TriggerCl.RefCollider.enabled = false;
        Rb.isKinematic = _t;
        Cl.enabled = true;
        TriggerCl.RefCollider.enabled = true;
    }

    private void TriggerCheck(Collider _cl)
    {
        if (_cl == TriggerBuffer)
        {
            return;
        }

        if (_cl.TryGetComponent(out IAutomation _i))
        {
            TriggerBuffer = _cl;
            PosRot _t = _i.Execute(this);

            Physics_Snap(_t.Position);
            Physics_Rotate(_t.Rotation * Vector3.forward, _t.Rotation * Vector3.up);

            return;
        }
        if (_cl.TryGetComponent(out Automation_LinearAutomation _s))
        {
            TriggerBuffer = _cl;
            _s.Execute(this);
            return;
        }
        if (_cl.TryGetComponent(out Automation_Pully _p))
        {
            TriggerBuffer = _cl;
            _p.Execute(this);
            return;
        }
        if (_cl.TryGetComponent(out Automation_GrindRail _gr))
        {
            TriggerBuffer = _cl;
            _gr.Execute(this, Rb.position);
            return;
        }
    }

    private void TriggerDCheck(Collider _)
    {
        TriggerBuffer = null;
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

        if (CurrentEstate is PlayerStates.Ground or PlayerStates.Roll or PlayerStates.Spindash)
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

        Gizmos.DrawWireSphere(transform.position + (PlayerDirection * homingDetectionDistance),
          homingDetectionRadius);

        Gizmos.DrawWireSphere(transform.position + (PlayerDirection * lightDetectionDistance),
            lightDetectionRadius);
    }

#endif

    #endregion Debug

    #region Moves

    public void HomingCheck()
    {
        HomingTargetDetector.Execute(transform.position, PlayerDirection);
    }

    public void RingCheck()
    {
        RingDetector.Execute(transform.position, PlayerDirection);
    }

    public void Jump()
    {
        VerticalVelocity += GroundNormal * Chp.JumpForce;
        Physics_ApplyVelocity();

        Jumping = true;
        MachineTransition(PlayerStates.Air);
    }

    public void Dash()
    {
        DashAction?.Invoke();

        if (Vector3.Dot(HorizontalVelocity, PlayerDirection) < Chp.DashSpeed)
        {
            HorizontalVelocity = PlayerDirection * Chp.DashSpeed;
        }
        else
        {
            HorizontalVelocity += PlayerDirection * Chp.DashBoost;
        }

        Physics_ApplyVelocity();
        MachineTransition(PlayerStates.Air);
    }

    #endregion Moves

    #endregion AdditionalFunctions

    public void OnEnable()
    {
        TriggerCl.TriggerEnter += TriggerCheck;
        TriggerCl.TriggerExit += TriggerDCheck;
    }

    public void OnDisable()
    {
        TriggerCl.TriggerEnter -= TriggerCheck;
        TriggerCl.TriggerExit -= TriggerDCheck;
    }
}

public enum PlayerStates
{
    Ground,
    Air,
    Win,
    Bounce,
    HomingAttack,
    LightSpeedDash,
    Roll,
    Spindash,
    Damage,
    RailGrinding,
    RailSwitch,
    LinearAutomation,
    Pully,
    Pole,
}