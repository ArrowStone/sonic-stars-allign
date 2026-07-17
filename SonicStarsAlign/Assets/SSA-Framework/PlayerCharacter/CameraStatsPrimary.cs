using System;
using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStats/Camera Primary Stats")]
public class CameraStatsPrimary : ScriptableObject
{
    [SerializeField]
    public CineCameraData CineCameraWhenBehind;
    public CineCameraData CineCameraWhenInFront;

    [System.Serializable]
    public class CineCameraData
    {
        public Vector2 screenPosition;
        public Cinemachine3OrbitRig.Settings OrbitNewSettings;

        [HideInInspector]
        public bool deadZone = true;
        [DrawTickBoxBefore("deadZone")]
        public Vector2 deadZoneSize = new Vector2(0.2f, 0.1f);

        [HideInInspector]
        public bool hardLimits = true;
        [DrawTickBoxBefore("hardLimits")]
        public Vector2 hardLimitsSize = new Vector2(0.8f, 0.6f);
        [DrawTickBoxBefore("hardLimits")]
        public Vector2 hardLimitsOffset = new Vector2(0f, 0f);

        public bool centerOnActivate = true;

        [Header("Target Tracking")]

        public Vector3 targetOffset = new Vector3();
        public Vector3 damping;
        [Range(0, 3)]
        public float lookAheadModifier;
    }

    public void ApplyComposerDataToComposer(ref Cinemachine3OrbitRig.Settings _CurrentOrbitSize, ref CinemachineOrbitalFollow OrbitalToOverwrite, ref CinemachineRotationComposer ComposerToOverwrite, ref float _currentLookAheadModifier,
            float lerpAmount)
    {
        // Framing / distance
        _CurrentOrbitSize.Top.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Top.Radius, CineCameraWhenInFront.OrbitNewSettings.Top.Radius, lerpAmount);
        OrbitalToOverwrite.Orbits.Top.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Top.Height, CineCameraWhenInFront.OrbitNewSettings.Top.Height, lerpAmount);
        _CurrentOrbitSize.Center.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Center.Radius, CineCameraWhenInFront.OrbitNewSettings.Center.Radius, lerpAmount);
        OrbitalToOverwrite.Orbits.Center.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Center.Height, CineCameraWhenInFront.OrbitNewSettings.Center.Height, lerpAmount);
        _CurrentOrbitSize.Bottom.Radius = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Bottom.Radius, CineCameraWhenInFront.OrbitNewSettings.Bottom.Radius, lerpAmount);
        OrbitalToOverwrite.Orbits.Bottom.Height = Mathf.Lerp(CineCameraWhenBehind.OrbitNewSettings.Bottom.Height, CineCameraWhenInFront.OrbitNewSettings.Bottom.Height, lerpAmount);

        ComposerToOverwrite.Composition.ScreenPosition = Vector2.Lerp(CineCameraWhenBehind.screenPosition, CineCameraWhenInFront.screenPosition, lerpAmount);

        // Dead zone
        ComposerToOverwrite.Composition.DeadZone.Enabled = lerpAmount < 0.5f ? CineCameraWhenBehind.deadZone : CineCameraWhenInFront.deadZone;
        ComposerToOverwrite.Composition.DeadZone.Size = Vector2.Lerp(CineCameraWhenBehind.deadZoneSize, CineCameraWhenInFront.deadZoneSize, lerpAmount);

        // Hard limits
        ComposerToOverwrite.Composition.HardLimits.Enabled = lerpAmount < 0.5f ? CineCameraWhenBehind.hardLimits : CineCameraWhenInFront.hardLimits;
        ComposerToOverwrite.Composition.HardLimits.Size = Vector2.Lerp(CineCameraWhenBehind.hardLimitsSize, CineCameraWhenInFront.hardLimitsSize, lerpAmount);
        ComposerToOverwrite.Composition.HardLimits.Offset = Vector2.Lerp(CineCameraWhenBehind.hardLimitsOffset, CineCameraWhenInFront.hardLimitsOffset, lerpAmount);

        // Activation behavior
        ComposerToOverwrite.CenterOnActivate = lerpAmount < 0.5f ? CineCameraWhenBehind.centerOnActivate : CineCameraWhenInFront.centerOnActivate;

        // Target tracking
        ComposerToOverwrite.TargetOffset = Vector3.Lerp(CineCameraWhenBehind.targetOffset, CineCameraWhenInFront.targetOffset, lerpAmount);
        OrbitalToOverwrite.TargetOffset = Vector3.Lerp(CineCameraWhenBehind.targetOffset, CineCameraWhenInFront.targetOffset, lerpAmount);
        OrbitalToOverwrite.TrackerSettings.PositionDamping = Vector3.Lerp(CineCameraWhenBehind.damping, CineCameraWhenInFront.damping, lerpAmount);

        // Lookahead
        _currentLookAheadModifier = Mathf.Lerp(CineCameraWhenBehind.lookAheadModifier, CineCameraWhenInFront.lookAheadModifier, lerpAmount);
    }
}