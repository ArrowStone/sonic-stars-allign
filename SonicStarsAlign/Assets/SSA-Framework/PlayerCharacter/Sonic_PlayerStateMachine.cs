using System;
using JetBrains.Annotations;
using Unity.Properties;
using UnityEngine;

public class Sonic_PlayerStateMachine : StateMachine_MonoBase<PlayerStates>
{
        public InputComponent Input;
        public Sonic_ModelManager ModelManager;

        public Transform InputRef;
        public Rigidbody Rb;
        public Animator Anim;
        public CapsuleCollider Cl;
        public Panel_Collider TriggerCl;
        [SerializeField] private PlayerCharacterParameters airChp;
        [SerializeField] private PlayerCharacterParameters waterChp;
        public PlayerCharacterParameters Chp;
        public PlayerCharacterStats Chs;
        public Sonic_SoundComponent Snd;
        public float InvinciblitiyState;
        public bool TrickState;

        [Header("Handling")]
        public AnimationCurve ChrTurn;

        [Header("Collision")]
        public LayerMask groundLayer;

        public LayerMask wallLayer;

        public LayerMask homingTargetLayer;

        public LayerMask ringLayer;

        public LayerMask railLayer;

        public LayerMask ledgeLayer;

        public LayerMask wallRunLayer;
        public LayerMask waterLayer;
        [Space]
        [SerializeField] private float homingDetectionDistance;

        [SerializeField] private float homingDetectionRadius;

        [SerializeField] private float lightDetectionDistance;

        [SerializeField] private float lightDetectionRadius;

        [SerializeField] private float railDetectionDistance;

        [SerializeField] private float railDetectionRadius;
        [SerializeField] public float railTimeout;
        public float railEndTime;
        [SerializeField] public Vector3 ledgeGrabDisplacement;
        [SerializeField] public Vector3 ledgeGrabReleaseDisplacement;

        [Space]
        [SerializeField] private float groundRayDig;

        [SerializeField] private float groundRayLength;
        [SerializeField] public float ledgeVerticalRayLength;
        [SerializeField] public float ledgeHorizontalRayLength;
        [Space]
        [SerializeField] public Transform ledgeVericalRayPoint;
        [SerializeField] public Transform ledgeHorizontalRayPoint;
        [SerializeField] private Vector3 normCollCenter;

        [SerializeField] private float normCollHeight;

        [Space]
        [SerializeField] private Vector3 crouchCollCenter;

        [SerializeField] private float crouchCollHeight;
        private bool _inWater;

        public bool InWater
        {
                get 
                {
                        return _inWater;
                }
                set
                {
                        _inWater = value;
                        if(value)
                        {
                                Chp = waterChp;
                        }
                        else
                        {
                                Chp = airChp;
                        }
                }
        }
        public bool runningOnWater;

        [SerializeField] public Vector3 WallRunNormal;
        [SerializeField] public bool WallRunDirection;
        [SerializeField] public bool OnWall;
        [SerializeField] public Vector3 ledgeGrabHorizontalVelocity;
        [SerializeField] public Vector3 ledgeGrabVerticalVelocity;
        [SerializeField] public float ledgeGrabStartTime;


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
        public Vector3 PreviousMoveDirection { get; set; }
        public float PlayerRunningSpeed { get; set; } = 0;

        public Vector3 Velocity {
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
        public Cast_Ray WaterCast { get; private set; }
        public Cast_Ray WallCast { get; private set; }
        public Cast_Ray CeilCast { get; private set; }
        public Overlap_Sphere RingDetector { get; private set; }
        public Overlap_Sphere HomingTargetDetector { get; private set; }
        public Overlap_Sphere RailDetectorL { get; private set; }
        public Overlap_Sphere RailDetectorR { get; private set; }
        public float MovementLockDistance;
        public Vector3 MovementLockStartPos;

        #endregion Util

        #region Moves

        public float airRotationSpeed; // For rotation toward the vertical position after leaving a ramp
        public bool doneAirRotation = true;
        public Vector3 fakeNormal;

        private bool _jumping;

        public bool Jumping {
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

        public bool DropDashing {
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

        public void ComponentSetup () {
                GroundCast = new Cast_Ray(groundRayLength, groundLayer);
                WaterCast = new Cast_Ray(groundRayLength, waterLayer, QueryTriggerInteraction.Collide);
                WallCast = new Cast_Ray(groundRayLength, wallLayer);
                CeilCast = new Cast_Ray(groundRayLength, wallLayer);
                HomingTargetDetector = new Overlap_Sphere(gameObject, 50, homingTargetLayer, homingDetectionDistance, homingDetectionRadius, wallLayer, DetectionBias.Proximity);
                RingDetector = new Overlap_Sphere(gameObject, 5, ringLayer, lightDetectionDistance, lightDetectionRadius, wallLayer, DetectionBias.Direction);

                RailDetectorL = new Overlap_Sphere(gameObject, 5, railLayer, railDetectionDistance, railDetectionRadius, wallLayer, DetectionBias.Proximity);
                RailDetectorR = new Overlap_Sphere(gameObject, 5, railLayer, railDetectionDistance, railDetectionRadius, wallLayer, DetectionBias.Proximity);

                SplnHandler = new SplineHandler();

                Chp = airChp;
        }

        public void StateSetup () {
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
                States.Add(PlayerStates.DropDash, new Sonic_DropDashState(this));
                States.Add(PlayerStates.SweepKick, new Sonic_SweepKickState(this));
                States.Add(PlayerStates.WallRun, new Sonic_WallRunState(this));
                States.Add(PlayerStates.WallJump, new Sonic_WallJumpState(this));
                States.Add(PlayerStates.LedgeGrab, new Sonic_LedgeGrabState(this));

                CurrentEstate = PlayerStates.Air;
                CurrentState = States[CurrentEstate];
                CurrentState.EnterState();
        }

        public void Start () {
                ComponentSetup();
                StateSetup();
                Initialize();
        }

        public void Update () {
                base.MachineUpdate();
                Chs.Shield?.Execute(Time.deltaTime);
        }

        public void FixedUpdate () {
                base.MachineFixedUpdate();

                Vector3 relevantVelocity = transform.InverseTransformDirection(Rb.linearVelocity);
                PlayerRunningSpeed = new Vector3 (relevantVelocity.x, 0, relevantVelocity.z).magnitude;
        }

        public void LateUpdate () {
                base.MachineLateUpdate();
        }

        private void Awake()
        {
            if (SceneSwitcher.Instance != null)
            {
                var selectedParams = SceneSwitcher.Instance.GetCachedCharacter();

                if (selectedParams != null)
                {
                    Chp = selectedParams;
                    airChp = selectedParams;
                    //waterChp = selectedParams;
                }

                var selectedStats = SceneSwitcher.Instance.GetCachedStats();

                if (selectedStats != null)
                {
                    Chs = selectedStats;
                }
            }
        }

    #region AdditionalFunctions

    public void Physics_ApplyVelocity () {
                Velocity = HorizontalVelocity + VerticalVelocity;
        }

        public void Respawn () {
                Player_StaticFunctions.SetTransform(transform, Chs.SpawnData.Position, Chs.SpawnData.Rotation, "Respawn");
                Death = false;
                MachineTransition(PlayerStates.Ground);
        }

        private Quaternion cashedRotation = Quaternion.identity;

        public Quaternion Physics_Rotate ( Vector3 _forward, Vector3 _up ) {
                Quaternion _diff = Quaternion.LookRotation(_forward, cashedRotation * Vector3.up);
                _diff = Quaternion.FromToRotation(_diff * Vector3.up, _up.normalized) * _diff;
                _diff *= Quaternion.Inverse(cashedRotation);

                Rb.MoveRotation(_diff * cashedRotation);
                cashedRotation = Rb.rotation;
                return _diff;
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

        public void Physics_Snap ( Vector3 _point, float _time ) {
                Vector3 _v = Vector3.Lerp(transform.position, _point, _time);
                if (!Physics_Sweep(_v, out _))
                {
                        Player_StaticFunctions.SetTransform(transform, _v, transform.rotation, "Physics Snap");
                }
        }

        public void ChangeKinematic ( bool _t ) {
                Cl.enabled = false;
                TriggerCl.RefCollider.enabled = false;
                Rb.isKinematic = _t;
                Cl.enabled = true;
                TriggerCl.RefCollider.enabled = true;
        }

        private void TriggerCheck ( Collider _cl ) {
                if (_cl == TriggerBuffer)
                {
                        return;
                }

                if (_cl.TryGetComponent(out Automation_Sound _sd))
                {
                        _sd.PlaySound();
                }
                if (_cl.TryGetComponent(out Automation_DashPanel_Rail _j))
                {
                        PosRot _t = _j.Execute(this);
                        Physics_Snap(_t.Position);
                        Physics_Rotate(_t.Rotation * Vector3.forward, _t.Rotation * Vector3.up);
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
                        _gr.Execute(this, transform.position);
                        return;
                }
        }

        private void TriggerDCheck ( Collider _ ) {
                TriggerBuffer = null;
        }

        #region Debug

#if UNITY_EDITOR

        public void OnDrawGizmos () {
                Debug.DrawLine(Rb.worldCenterOfMass, Rb.worldCenterOfMass + (InputRotation * Vector3.up * 1), Color.yellow);

                Debug.DrawRay(ledgeVericalRayPoint.position, Gravity * ledgeVerticalRayLength, Color.red);
                Debug.DrawRay(ledgeHorizontalRayPoint.position, transform.forward * ledgeHorizontalRayLength, Color.red);

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

                Gizmos.DrawWireSphere(transform.position + (-transform.right * railDetectionDistance),
                    railDetectionRadius);

                Gizmos.DrawWireSphere(transform.position + (transform.right * railDetectionDistance),
                    railDetectionRadius);
        }

#endif

        #endregion Debug

        #region Moves

        public void HomingCheck () {
                HomingTargetDetector.Execute(transform.position - Gravity * 5f, PlayerDirection);
        }

        public void RingCheck () {
                RingDetector.Execute(transform.position, PlayerDirection);
        }

        public void RailCheck () {
                RailDetectorL.Execute(transform.position, -transform.right);
                RailDetectorR.Execute(transform.position, transform.right);
        }

        public void Jump () {
                Snd.PlaySound("Jump");
                Snd.PlaySound("JumpVoiceLine");

                VerticalVelocity += GroundNormal * Chp.JumpForce;
                Physics_ApplyVelocity();

                Jumping = true;
                ModelManager.EnterBall();
                MachineTransition(PlayerStates.Air);
        }

        public void Dash () {
                MachineTransition(PlayerStates.Air);
                if(InputVector.magnitude > 0) PlayerDirection = InputVector;
                DashAction?.Invoke();

                /*if (Vector3.Dot(HorizontalVelocity, PlayerDirection) > Chp.DashSpeed)
                {
                        HorizontalVelocity = PlayerDirection * Chp.DashSpeed;
                }
                else
                {
                        HorizontalVelocity += PlayerDirection * Chp.DashBoost;
                }*/
                // I dash => I want to go in a particular direction
                HorizontalVelocity = PlayerDirection * Chp.DashSpeed;
                Debug.Log(VerticalVelocity);

                Physics_ApplyVelocity();
                ModelManager.ExitBall();
                Anim.SetInteger("State", 0);
        }

        #endregion Moves

        #endregion AdditionalFunctions

        public void OnEnable () {
                TriggerCl.TriggerEnter += TriggerCheck;
                TriggerCl.TriggerExit += TriggerDCheck;
        }

        public void OnDisable () {
                TriggerCl.TriggerEnter -= TriggerCheck;
                TriggerCl.TriggerExit -= TriggerDCheck;
        }

        private void OnTriggerEnter ( Collider other ) {
                if (FrameworkUtility.CompareLayer(other.gameObject.layer, waterLayer))
                {
                        InWater = true;
                }
        }

        private void OnTriggerExit ( Collider other ) {
                if (FrameworkUtility.CompareLayer(other.gameObject.layer, waterLayer))
                {
                        InWater = false;
                }
        }

        public bool CanRunOnWater ()
        {
                // Needs enough horizontal speed
                if (HorizontalVelocity.magnitude < Chp.WaterRunThreshold) return false;

                // Standing "on" water surface, not submerged
                return true;
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
        DropDash,
        SweepKick,
        WallRun,
        WallJump,
        LedgeGrab,
}