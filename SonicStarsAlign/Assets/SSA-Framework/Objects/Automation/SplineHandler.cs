using UnityEngine;
using UnityEngine.Splines;

public class SplineHandler
{
    public bool Active;
    public SplineContainer ActiveSpline;
    public SplineType ActiveSplineType;
    public bool Forward;
    public float SpeedFactor;
    public bool Loose;
    private bool looping;

    private Vector3 offset;
    public float SpeedMultiplier;
    public AnimationCurve SpeedOverTime;
    private float time;

    public void SplineSetup(SplineContainer _spline, SplineType _type, AnimationCurve _speedCurve,
        float _speedMultiplier, Vector3 _offset, float _startTime = 0, bool _looping = false, bool _loose = false)
    {
        Active = true;
        ActiveSpline = _spline;
        ActiveSplineType = _type;
        SpeedOverTime = _speedCurve;
        SpeedMultiplier = _speedMultiplier;

        offset = _offset;
        Loose = _loose;
        looping = _looping;
        time = _startTime;
        Forward = SpeedMultiplier * SpeedOverTime.Evaluate(time) > 0;
    }

    public void SplineMove(float _deltaTime)
    {
        if (!Active) return;
        SpeedFactor = SpeedMultiplier * SpeedOverTime.Evaluate(time) * _deltaTime / ActiveSpline.CalculateLength();
        time += SpeedFactor;
        Forward = SpeedFactor >= 0;

        if (Forward)
        {
            if (time >= 1 && !looping)
            {
                Active = false;
                return;
            }
            if (time >= 1 && looping) time--;
        }
        else
        {
            if (time <= 0 && !looping)
            {
                Active = false;
                return;
            }
            if (time <= 0 && looping) time++;
        }
    }

    public Vector3 NewPosition()
    {
        return (Vector3)ActiveSpline.EvaluatePosition(Mathf.Clamp01(time)) + offset.y * SplineNormal();
    }

    private Vector3 PreviousNormal = Vector3.up;

    public Vector3 SplineNormal()
    {
        if (Mathf.Approximately(Mathf.Clamp01(time), 0) || Mathf.Approximately(Mathf.Clamp01(time), 1))
        {
            return PreviousNormal;
        }
        PreviousNormal = ActiveSpline.EvaluateUpVector(Mathf.Clamp01(time));
        return PreviousNormal;
    }

    public void SetTangent(Vector3 Tangent)
    {
        PreviousTangent = Tangent;
    }

    private Vector3 PreviousTangent;

    public Vector3 SplineTangent()
    {
        if (Mathf.Approximately(Mathf.Clamp01(time), 0) || Mathf.Approximately(Mathf.Clamp01(time), 1))
        {
            return PreviousTangent;
        }
        PreviousTangent = ActiveSpline.EvaluateTangent(Mathf.Clamp01(time));
        return PreviousTangent;
    }

    public void Clear()
    {
        ActiveSpline = null;
        Active = false;
        SpeedOverTime = null;
        SpeedMultiplier = 0;
        time = 0;
        offset = Vector3.zero;
    }
}

public enum SplineType
{
    Spring,
    DashRamp,
    DashRing,
    GrindRail
}