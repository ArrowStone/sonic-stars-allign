using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class Automation_Pully : MonoBehaviour
{
    public SplineContainer RefSpline;
    public GameObject RefHandle;

    [Space]
    [SerializeField] private Vector3 offset;

    [Space]
    [SerializeField] private float resetTime;

    [SerializeField] private float resetWait;
    private float _rset;
    [SerializeField] private float resetSpeed;

    [Space]
    public UnityEvent InteractionEvent;

    #region Util

    private float _time;
    public bool _active;

    #endregion Util

    public void Execute(Sonic_PlayerStateMachine _ctx)
    {
        if (RefSpline == _ctx.SplnHandler.ActiveSpline)
        {
            return;
        }

        _active = true;
        _rset = 0;

        InteractionEvent.Invoke();
        Vector3 tangent = RefSpline.EvaluateTangent(_time).xyz;

        _ctx.SplnHandler._ctxPully = this;
        _ctx.CurrentState.ExitState();
        _ctx.SplnHandler.SplineSetup(RefSpline, SplineType.Pully, AnimationCurve.Constant(0, 1, 1), Vector3.Dot(_ctx.Velocity, tangent.normalized), offset, _time, RefSpline.Spline.Closed);
        _ctx.SplnHandler.SetTangent(tangent);
        _ctx.MachineTransition(PlayerStates.Pully);
    }

    public void Update()
    {
        if (_active) return;

        _rset += Time.deltaTime;
        if (_rset >= resetWait)
        {
            _time = Mathf.MoveTowards(_time, resetTime, resetSpeed * Time.deltaTime);
        }
        VisualUpdate(_time);
    }

    public void VisualUpdate(float _t)
    {
        RefSpline.Evaluate(_t, out float3 pos, out float3 _f, out float3 _up);
        Quaternion _rot = Quaternion.LookRotation(_f, _up);
        RefHandle.transform.SetPositionAndRotation(pos, _rot);
        _time = _t;
    }
}