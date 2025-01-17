using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Automation_TrickRamp : MonoBehaviour, IAutomation
{
    public float2 Speed;
    public Vector3 Force;
    public Vector3 ForceMax;

    Sonic_PlayerStateMachine cashedPlayer;

    [Space]
    public bool Set;

    public UnityEvent InteractionEvent;

    public PosRot Execute(Sonic_PlayerStateMachine _ctx)
    {
        cashedPlayer = _ctx;
        cashedPlayer.JumpAction += Action;

        PosRot _transfrm = new()
        {
            Position = _ctx.Rb.position,
            Rotation = Quaternion.LookRotation(_ctx.PlayerDirection, _ctx.GroundNormal)
        };
        return _transfrm;
    }

    public void OnTriggerExit(Collider other)
    {
        cashedPlayer.JumpAction -= Action;
    }

    public void Action()
    {
        if (cashedPlayer.HorizontalVelocity.magnitude > Speed.x)
        {
            float _t = (Mathf.Clamp(cashedPlayer.HorizontalVelocity.magnitude, Speed.x, Speed.y) - Speed.x) / (Speed.y - Speed.x);

            Vector3 _fr = Vector3.Lerp(Force, ForceMax, _t);
            Vector3 _vel = transform.rotation * _fr;

            Debug.Log(_t, this);
            Debug.Log(_vel, this);

            cashedPlayer.Jumping = false;

            cashedPlayer.Velocity = _vel;
            FrameworkUtility.SplitPlanarVector(cashedPlayer.Velocity, -cashedPlayer.Gravity.normalized, out var _v, out var _h);

            cashedPlayer.VerticalVelocity = _v;
            cashedPlayer.HorizontalVelocity = _h;
        }

        Debug.Log("i");
        cashedPlayer.JumpAction -= Action;
    }
}