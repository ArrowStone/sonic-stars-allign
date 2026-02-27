using UnityEngine;

public class CamBrain : StateMachine_MonoBase<CameraStates>
{
        [SerializeField,TextSpace("This component determines and calls the camera's state (between alive, dead and transitioning), and applies transformations.")]
        bool DummyText;

        public Transform CamTransform;
        public InputComponent Input;

        public bool SetPositionAndRotationManually;

        public ICamPointStyle Point;
        public AnimationCurve WeightCurve;

        #region Util

        public PosRot CashedTransform { get; set; }

        #endregion Util

        private void Awake () {
                Input = GameObject.Find("Player_Rigidbody").GetComponent<Sonic_PlayerStateMachine>().Input;

                CashedTransform = new()
                {
                        Position = CamTransform.position,
                        Rotation = CamTransform.rotation,
                };
                StateSetup();
                Initialize();
        }

        public void StateSetup () {
                States.Add(CameraStates.Alive, new Camera_AliveState(this));
                States.Add(CameraStates.Dead, new Camera_DeadState(this));
                States.Add(CameraStates.Transitioning, new Camera_TransitionState(this));

                CurrentEstate = CameraStates.Dead;
                CurrentState = States[CurrentEstate];
                CurrentState.EnterState();
        }

        private void Update () {
                MachineUpdate();
        }

        private void FixedUpdate () {
                MachineFixedUpdate();
        }

        private void LateUpdate () {
                MachineLateUpdate();
        }

        #region Functions

        public void ApplyPoint () {
                CashedTransform = Point.Transform();

                if(SetPositionAndRotationManually)
                        CamTransform.SetPositionAndRotation(CashedTransform.Position, CashedTransform.Rotation);
        }

        public void SetPoint (ICamPointStyle NewPoint) {
                Point.OnExitPoint();
                Point = NewPoint;
                Point.OnEnterPoint(this);
        }

        #endregion Functions
}

public enum CameraStates
{
        Alive,
        Dead,
        Transitioning,
}