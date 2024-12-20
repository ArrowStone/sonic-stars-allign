using UnityEngine;

public static class FrameworkUtility
{
    public const double ESlipAngle = 45d;
    public const double EFallAngle = 70d;

    public const double FloorAngle = 23d;
    public const double SlopeAngle = 80d;
    public const double SteepAngle = 156d;

    public const double _deadZone = 0.1;

    public static double Smooth(float t, float f = 0.5f, float a1 = 0.1f)
    {
        return t == 1 ? 1
            : Mathf.Clamp01(((1 / (1 - t)) - 1) * Mathf.Pow(1 - Mathf.Pow(1 - f, Time.deltaTime), a1));
    }

    public static void SplitPlanarVector(Vector3 _vector, Vector3 _normal, out Vector3 _projectedOutput, out Vector3 _verticalOutput)
    {
        _projectedOutput = Vector3.ProjectOnPlane(_vector, _normal);
        _verticalOutput = _vector - _projectedOutput;
    }

    public static bool IsApproximate(Vector3 a, Vector3 b, float _deadZone)
    {
        Vector3 Difference = a - b;
        return Difference.magnitude <= _deadZone;
    }

    public static Vector3 DivideVector3(Vector3 a, Vector3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z);

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    public static double QuaternionMagnitude(Quaternion q)
    {
        return Mathf.Sqrt((q.w * q.w) + (q.x * q.x) + (q.y * q.y) + (q.z * q.z));
    } 
}