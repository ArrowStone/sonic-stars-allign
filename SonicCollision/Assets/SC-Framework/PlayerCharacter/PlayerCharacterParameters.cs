using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStats/Player Parameters")]
public class PlayerCharacterParameters : ScriptableObject
{
    [Header("Handling")]
    public float BaseSpeed;

    public float HardSpeedCap;

    public float RailSpeedCap;

    public float Acceleration;

    public float Deceleration;

    public float GroundDrag;

    public float RunSpeedThreshold = 12f; // Minimum speed needed to trigger SweepKick

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
    public float RotationSmoothingSpeed;

    [Space]
    public float JumpForce;
    public float JumpGravityScale;

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
    public float SlopeFactorDown;

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

    public float DashLength;

    public float HomingAttackSpeed;
    public float HomingAttackBounceForce;
    public float HomingAttackSpeedMultiplier;
    public float RailHomingTargetOffset;

    [Space]
    [Header("LightDashSpeed")]
    public float LightDashSpeed;
    public float LightDashExitSpeed;

    [Space]
    [Header("Rail")]
    public float RailCollisionBounce;

    public float RailSwitchSpeed;
    public float RailSwitchDuration;
    public float RailSwitchDeadZone;
    public float RailTrickSpeed;

    [Space]
    [Header("Drop Dash")]
    public float DropDashBaseSpeed = 8f; // minimum speed when landing
    public float DropDashChargeMultiplier = 4f; // how much extra speed is added per second charged
    public float DropDashMaxCharge = 1.0f; // max seconds chargeable

    [Space]
    [Header("Water")]
    public float WaterRunThreshold = 14f;      // min speed needed to run across water

    [Space]
    [Header("Wall Run")]
    public float WallAttachCheckDistance = 1.0f;
    public float WallRunGravityScale = 0.35f;
    public float WallRunSpeed = 12f;
    public float WallJumpStrength = 18f;
    public float MaxWallRunTime = 2.0f;
    public float MinWallDot = 0.5f; // 0 = flat ground, 1 = vertical
    public float MaxWallGravityDot = 0.2f;
    public float WallRunMinSpeed = 8f;
    public float MaxWallHeadOnDot = 0.7f;
    public float WallRunVerticalControl = 4f; // how much player can steer up/down

    [Space]
    [Header("Wall Jump")]
    public float WallJumpMaxCharge = 1.0f;       // how long you can hold jump to charge
    public float WallJumpMinStrength = 10f;      // smallest jump strength
    public float WallJumpMaxStrength = 25f;      // largest jump strength

    [Space]
    [Header("Ledge Grab")]
    public AnimationCurve LedgeGrabVelocityDecrease;

    [Space]
    [Header("Stone Skipping")]
    public float stoneSkipMinimumSpeed = 14f;   // minimum speed for stone skipping
    public float stoneSkipWindow = 0.5f;        // the number of seconds you need to press the button before hitting the water
    public float stoneSkipCooldown = 1f;        // cooldown to prevent spamming
    public float stoneSkipHorizontalSpeedMultiplier = 1.2f;    // how much your horizontal speed increases by

    [Space]
    [Header("Prop Surf")]
    public float MinSurfSpeed;
    public float SurfAcceleration;
    public float SurfSkidThreshold;
    public float SurfSkidAcceleration;
    public float SurfSkidDeccelerationMultiplier;
}