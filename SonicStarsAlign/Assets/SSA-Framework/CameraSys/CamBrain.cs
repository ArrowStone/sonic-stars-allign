using Unity.VisualScripting;
using UnityEngine;

public class CamBrain : StateMachine_MonoBase<CameraStates>
{
    public Camera Cam;
    public InputComponent Input;

    public ICamPoint Point;
    public AnimationCurve WeightCurve;

    #region Util

    public PosRot CashedTransform { get; set; }

    #endregion Util

    private void Awake()
    {
        CashedTransform = new()
        {
            Position = Cam.transform.position,
            Rotation = Cam.transform.rotation,
        };
        StateSetup();
        Initialize();
    }

    public void StateSetup()
    {
        States.Add(CameraStates.Alive, new Camera_AliveState(this));
        States.Add(CameraStates.Dead, new Camera_DeadState(this));
        States.Add(CameraStates.Transitioning, new Camera_TransitionState(this));

        CurrentEstate = CameraStates.Dead;
        CurrentState = States[CurrentEstate];
        CurrentState.EnterState();
    }

    private void Update()
    {
        MachineUpdate();
    }

    private void LateUpdate()
    {
        MachineLateUpdate();
    }

    #region Functions

    public void ApplyPoint()
    {
        CashedTransform = Point.Transform();
        Cam.transform.SetPositionAndRotation(CashedTransform.Position, CashedTransform.Rotation);
    }

    #endregion Functions
}

public enum CameraStates
{
    Alive,
    Dead,
    Transitioning,
}