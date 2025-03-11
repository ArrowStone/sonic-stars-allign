using System;
using UnityEngine;

[CreateAssetMenu]
public class PlayerCharacterParameters : ScriptableObject
{
    [Header("Handling")]
    public float BaseSpeed;

    public float HardSpeedCap;

    public float RailSpeedCap;

    public float Acceleration;

    public float Deceleration;

    public AnimationCurve TurnDeceleration;

    public AnimationCurve TurnStrengthCurve;

    [Space]
    public float BaseSpeedAir;

    public AnimationCurve AirDrag;

    public float AccelerationAir;

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

    public float JumpCancel;

    [Space]
    public float SlipTime;

    public float LookBackTransitionSpeed;

    [Space]
    [Header("Gravity")]
    public float GravityForce;

    public float FallVelCap;

    [Space]
    [Header("SlopePhysics")]
    public float MinGroundStickSpeed;

    public float MaxGroundDeviation;

    [Space]
    public float SlopeFactor;

    public float SlopeFactorRoll;

    public float SlopeFactorRollDown;

    [Space]
    public float RailSlopeInfluence;

    public float RailCrouchInfluence;

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

    public AnimationCurve BounceFactor;

    public float MaxBounceHieght;

    [Space]
    [Header("Attack")]
    public float DashSpeed;

    public float DashBoost;

    public float HomingAttackSpeed;

    [Space]
    [Header("LightDashSpeed")]
    public float LightDashSpeed;

    [Space]
    [Header("Rail")]
    public float RailCollisionBounce;
    public float RailSwitchSpeed;
    public float RailSwitchDuration;
    public float RailSwitchDeadZone;
}