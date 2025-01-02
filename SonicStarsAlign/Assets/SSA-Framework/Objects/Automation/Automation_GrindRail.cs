using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Events;

public class Automation_GrindRail : MonoBehaviour
{
    public SplineContainer RefSpline;

    [SerializeField]
    private Vector3 offset;

    [Space]

    public UnityEvent InteractionEvent;

    #region Util
    private const int _res = 10;
    private const int _iterations = 4;
    private float _time;
    #endregion Util

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.TryGetComponent(out Sonic_PlayerStateMachine _target))
        {
            Execute(_target, _target.Rb.position);
        }
    }

    public void Execute(Sonic_PlayerStateMachine _ctx, float3 point)
    {
        if (RefSpline == _ctx.SplnHandler.ActiveSpline)
        {
            return;
        }

        InteractionEvent.Invoke();
        float3 inverseLocalPoint = RefSpline.transform.InverseTransformPoint(point);
        _ = SplineUtility.GetNearestPoint(RefSpline.Spline, inverseLocalPoint, out _, out _time, _res, _iterations);

        Vector3 tangent = RefSpline.EvaluateTangent(_time).xyz;

        _ctx.SplnHandler.SplineSetup(RefSpline, SplineType.GrindRail, AnimationCurve.Constant(0, 1, 1), Vector3.Dot(_ctx.Rb.linearVelocity, tangent.normalized), offset, _time, RefSpline.Spline.Closed);
        _ctx.SplnHandler.SetTangent(tangent);
        _ctx.MachineTransition(PlayerStates.RailGrinding);
    }
}