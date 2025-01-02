using UnityEngine;

public class CamBrain : StateMachine_MonoBase<CameraStates>
{
    public Camera Cam;
    public InputComponent Input;

    public ICamPoint Point;

    #region Util

    public Vector3 CashedPosition { get; set; }
    public Quaternion CashedRotation { get; set; }

    #endregion Util

    private void Awake()
    {
        CashedPosition = Cam.transform.position;
        CashedRotation = Cam.transform.rotation;
        StateSetup();
        Initialize();
    }

    public void StateSetup()
    {
        States.Add(CameraStates.Alive, new Camera_AliveState(this));
        States.Add(CameraStates.Dead, new Camera_DeadState(this));
        States.Add(CameraStates.Transitioning, new Camera_AliveState(this));

        CurrentEstate = CameraStates.Dead;
        CurrentState = States[CurrentEstate];
        CurrentState.EnterState();
    }

    private void Update()
    {
        MachineUpdate();
    }

    #region Functions

    public void ApplyPoint()
    {
        Cam.transform.SetPositionAndRotation(Point.Position(), Point.Rotation());
        CashedPosition = Cam.transform.position;
        CashedRotation = Cam.transform.rotation;
    }

    #endregion Functions
}

public enum CameraStates
{
    Alive,
    Dead,
    Transitioning,
}