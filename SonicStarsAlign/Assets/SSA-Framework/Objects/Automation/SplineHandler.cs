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
    private int spln;

    private Vector3 offset;
    public float SpeedMultiplier;
    public AnimationCurve SpeedOverTime;
    public float Time;

    #region Misc

    public Automation_GrindRail SwitchDir;
    public Automation_Pully _ctxPully;
    public Automation_PoleSwing _ctxPole;

    #endregion Misc

    public void SplineSetup(SplineContainer _spline, SplineType _type, AnimationCurve _speedCurve, float _speedMultiplier, Vector3 _offset, float _startTime = 0, bool _looping = false, bool _loose = false, int _spln = 0)
    {
        Active = true;
        ActiveSpline = _spline;
        spln = _spln;
        ActiveSplineType = _type;
        SpeedOverTime = _speedCurve;
        SpeedMultiplier = _speedMultiplier;

        offset = _offset;
        Loose = _loose;
        looping = _looping;
        Time = _startTime;
        Forward = SpeedMultiplier * SpeedOverTime.Evaluate(Time) > 0;
    }

    public void SplineMove(float _deltaTime)
    {
        if (!Active) return;
        SpeedFactor = SpeedMultiplier * SpeedOverTime.Evaluate(Time) * _deltaTime / ActiveSpline.CalculateLength();
        Time += SpeedFactor;
        Forward = SpeedFactor >= 0;

        if (Forward)
        {
            if (Time >= 1 && !looping)
            {
                Active = false;
                return;
            }
            if (Time >= 1 && looping) Time--;
        }
        else
        {
            if (Time <= 0 && !looping)
            {
                Active = false;
                return;
            }
            if (Time <= 0 && looping) Time++;
        }
    }

    public Vector3 NewPosition()
    {
        return ActiveSpline.transform.TransformPoint((Vector3)ActiveSpline[spln].EvaluatePosition(Mathf.Clamp01(Time)) + offset.y * SplineNormal());
    }

    public Vector3 GetPosition(SplineContainer _s, float _t)
    {
        return _s.transform.TransformPoint((Vector3)_s[0].EvaluatePosition(Mathf.Clamp01(_t)) + offset.y * SplineNormal());
    }

    private Vector3 PreviousNormal = Vector3.up;

    public Vector3 SplineNormal()
    {
        if (Mathf.Approximately(Mathf.Clamp01(Time), 0) || Mathf.Approximately(Mathf.Clamp01(Time), 1))
        {
            return PreviousNormal;
        }
        PreviousNormal = ActiveSpline.EvaluateUpVector(Mathf.Clamp01(Time));
        return PreviousNormal;
    }

    public void SetTangent(Vector3 Tangent)
    {
        PreviousTangent = Tangent;
    }

    private Vector3 PreviousTangent;

    public Vector3 SplineTangent()
    {
        if (Mathf.Approximately(Mathf.Clamp01(Time), 0) || Mathf.Approximately(Mathf.Clamp01(Time), 1))
        {
            return PreviousTangent;
        }
        PreviousTangent = ActiveSpline.EvaluateTangent(Mathf.Clamp01(Time));
        return PreviousTangent;
    }

    public void Clear()
    {
        ActiveSpline = null;
        Active = false;
        SpeedOverTime = null;
        SpeedMultiplier = 0;
        Time = 0;
        offset = Vector3.zero;
    }
}

public enum SplineType
{
    Spring,
    DashRamp,
    DashRing,
    GrindRail,
    Pully,
    Pole,
}