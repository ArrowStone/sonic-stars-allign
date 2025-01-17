using UnityEngine;

public static class FrameworkUtility
{
    public const double ESlipAngle = 45d;
    public const double EFallAngle = 70d;

    public const double FloorAngle = 23d;
    public const double SlopeAngle = 80d;
    public const double SteepAngle = 156d;

    public const double _deadZone = 0.1;

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
}
public struct PosRot
{
    public Vector3 Position;
    public Quaternion Rotation;
}