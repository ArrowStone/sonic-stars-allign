using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStats/Camera Effects Stats")]
public class CameraStatsEffects : ScriptableObject
{
        public LayerMask CameraCollidesWith;

        [Header("Dynamic FOV and Distance")]
        public TypeOfFOVLerp HowToAdjustFOV;

        public enum TypeOfFOVLerp
        {
                lerp,
                moveTowards
        }

        [OnlyDrawIf("HowToAdjustFOV", TypeOfFOVLerp.lerp)]
        [Tooltip("X is how much to lerp fov and distance by when accelerating, y is when decelerating.")]
        public Vector2 lerpSpeeds = new Vector2(0.2f, 0.3f);
        [Tooltip("No matter the lerp, FOV will always change by this amount at least. Note that both of these values will be affected by delta time.")]
        [OnlyDrawIf("HowToAdjustFOV", TypeOfFOVLerp.lerp)]
        public Vector2 minAmountToAdjustFOV = new Vector2(5f, 10f);
        //[Tooltip("No matter lerp, distance modifier will always change by this amount at least. Note that both of these values will be affected by delta time.")]
        //[OnlyDrawIf("HowToAdjustFOV", TypeOfFOVLerp.lerp)]
        //public Vector2 minAmountToAdjustDistance = new Vector2(0.04f, 0.4f);

        [Tooltip("X is how much many values to adjust fov by when accelerating, y is when decelerating. Note that both of these values will be affected by delta time.")]
        [OnlyDrawIf("HowToAdjustFOV", TypeOfFOVLerp.moveTowards)]
        public Vector2 moveTowardsFOVSpeeds = new Vector2(1, 2);
        //[Tooltip("X is how much many values to adjust distance by when accelerating, y is when decelerating. Note that both of these values will be affected by delta time.")]
        //[OnlyDrawIf("HowToAdjustFOV", TypeOfFOVLerp.moveTowards)]
        //public Vector2 moveTowardsDistanceModSpeeds = new Vector2(0.05f, 0.1f);

        public AnimationCurve FOVBySpeed = new AnimationCurve( new Keyframe[] {
                new Keyframe(0, 70),
                new Keyframe(25, 90f),
        } );
        public AnimationCurve DistanceModifierByFOV = new AnimationCurve( new Keyframe[] {
                new Keyframe(70, 1f),
                new Keyframe(90, 0.5f),
        } );
        [Header("Tracking and view")]
        [Tooltip("The x is the angle difference between camera and characters current 'up' vector, 0 - 180. Y is the vertical offset of the target up or down.")]
        public AnimationCurve VerticalOffsetByViewAngle = new AnimationCurve( new Keyframe[] {
                new Keyframe(0, 0.3f),
                new Keyframe(90, 0),
                new Keyframe(180, 0.3f),
        } );

        [Header("Recentering")]
        [Tooltip("The X is the min speed to enable recentering, the Y is the speed to disable recenter when slower than.")]
        public Vector2 MinSpeedToAutoRecenter;
}