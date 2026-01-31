using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class CamPoint_FollowSpline : MonoBehaviour, ICamPoint
{
    public CamBrain Brain;

    [Space]
    [Header("Parameters")]
    public Transform Target;
    private Rigidbody targetRb;

    public SplineContainer TargetSpline;
    public float FollowDistance; // Offset in units
    private float followOffset; // Offset in relation to total length
    public float3 RotationOffset;

    [Space]
    public float RotationSmoothTime = 0.2f;

    [Space]
    public float RecenteringWait;

    public float RecenteringSpeed;

    #region Util

    private Vector3 _cashedTargetPosition;
    private Vector2 _rot;

    private float3 nearest;
    private float t;

    #endregion Util

    public void OnEnter(CamBrain _brain)
    {
        targetRb = Target.GetComponent<Rigidbody>();
        Brain = _brain;
        _position = _brain.CashedTransform.Position;
        _rotation = _brain.CashedTransform.Rotation;
        _cashedTargetPosition = Target.position;

        followOffset = -FollowDistance / TargetSpline.Spline.GetLength();
    }

    public void Execute(float _delta)
    {
        if (Target != null)
        {
            _cashedTargetPosition = Target.position;
        }

        _position = UpdatePosition(_delta);
        _rotation = UpdateRotation(_delta);
    }

    public void OnExit()
    {
        Brain = null;
    }

    #region AdditionalFunctions

    public Vector3 UpdatePosition(float _delta)
    {
        SplineUtility.GetNearestPoint(TargetSpline.Spline, TargetSpline.transform.worldToLocalMatrix.MultiplyPoint(targetRb.transform.position), out nearest, out t);
        t += followOffset;
        Vector3 output = TargetSpline.Spline.EvaluatePosition(t);
        output = TargetSpline.transform.localToWorldMatrix.MultiplyPoint(output);
        return output;
    }

    public Quaternion UpdateRotation(float _delta)
    {
        return Quaternion.RotateTowards(_rotation, Quaternion.LookRotation(TargetSpline.Spline.EvaluateTangent(t) + RotationOffset), RotationSmoothTime * _delta);
    }

    #endregion AdditionalFunctions

    private Vector3 _position;

    private Quaternion _rotation = Quaternion.identity;

    public PosRot Transform()
    {
        Quaternion _rotRot = new()
        {
            eulerAngles = new Vector3(_rot.x, _rot.y)
        };
        PosRot _transfrm = new PosRot()
        {
            Position = _position,
            Rotation = _rotation * _rotRot,
        };
        return _transfrm;
    }
}