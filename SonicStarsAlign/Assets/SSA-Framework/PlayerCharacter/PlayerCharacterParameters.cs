using System;
using UnityEngine;

[CreateAssetMenu]
public class PlayerCharacterParameters : ScriptableObject
{
    [Header("Handling")]
    public float BaseSpeed;

    public float SoftSpeedCap;

    public float SpeedCapStrength;

    public float HardSpeedCap;

    public AnimationCurve AccelerationCurve;

    public AnimationCurve DecelerationCurve;

    public AnimationCurve TurnDeceleration;

    public AnimationCurve TurnStrengthCurve;

    [Space]
    public float BaseSpeedAir;

    public AnimationCurve AccelerationCurveAir;

    public AnimationCurve DecelerationCurveAir;

    public AnimationCurve TurnDecelerationAir;

    public AnimationCurve TurnStrengthCurveAir;

    [Space]
    [Range(-1f, 1f)]
    public float TurnDeviationCap;

    public float MinBreakSpeed;

    public float BreakStrength;

    public float BreakStrengthAir;

    [Space]
    public float JumpForce;

    public float JumpCancelStrength;

    [Space]
    public float SlipTime;

    [Space]
    [Header("Gravity")]
    public float GravityForce;

    public float FallSpeedCap;

    [Space]
    [Header("SlopePhysics")]
    public float MinGroundStickSpeed;

    public float MaxGroundDeviation;

    [Space]
    public float SlopeFactor;

    public float SlopeFactorRoll;

    public float SlopeFactorRollDown;

    [Space]
    [Header("Roll")]
    public float RollDeceleration;

    public float SpinDashDeceleration;

    public float MinRollSpeed;

    [Space]
    public float SpinDashHoldTime;

    public float SpinDashInitSpeed;

    public AnimationCurve SpinDashOutput;
    public AnimationCurve DropDashOutput;

    [Space]
    [Header("Bounce")]
    public float BounceSpeed;    
    public float MaxBounceSpeed;    
    public float BounceGravity;    
    public float BounceFactor;
    public float MaxBounceHieght;
}