using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Movement_Aerial : MonoBehaviour, Enemy_MovementBase
{
        private Rigidbody Rb;
        public Transform target;
        public LayerMask targetLayer;
        public LayerMask detectionBlockingMask;
        public float detectionDistance;
        private Overlap_Sphere detector;
        private Quaternion targetRotation;
        public float movementSpeed;
        public float rotationSpeed;
        public SplineContainer PathContainer;
        private Spline path;
        private float pathProgress = 0;
        private bool onPath = false;
        float3 pathPosition;
        float3 cachedPathPosition;

        void Awake () {
                detector = new Overlap_Sphere(gameObject, 1, targetLayer, 0, detectionDistance, detectionBlockingMask, DetectionBias.Proximity);
                Rb = GetComponent<Rigidbody>();
                path = PathContainer.Spline;
                pathPosition = transform.position;
                cachedPathPosition = pathPosition;
        }

        void FixedUpdate () {
                float _delta = Time.fixedDeltaTime;

                detector.Execute(transform.position, Vector3.forward);
                if (detector.TargetDetected) target = detector.TargetOutput.transform;
                // The sphere doesn't detect player if they're too close for some reason
                else if (target && Vector3.Distance(target.position, transform.position) > detectionDistance) target = null;

                Vector3 targetLookPos;

                if (target)
                {
                        targetLookPos = target.position;
                        onPath = false;
                }
                else
                {
                        // Idling behaviour

                        cachedPathPosition = pathPosition;

                        if (onPath)
                        {
                                pathPosition = PathContainer.transform.localToWorldMatrix.MultiplyPoint(path.EvaluatePosition(pathProgress));
                                if (Vector3.Distance(transform.position, pathPosition) > 5f)
                                {
                                        pathPosition = cachedPathPosition;
                                }
                                else
                                {
                                        pathProgress += 0.001f;
                                        if (pathProgress > 1) pathProgress -= 1f;
                                }
                                targetLookPos = pathPosition;
                        }
                        else
                        {
                                SplineUtility.GetNearestPoint(path, PathContainer.transform.worldToLocalMatrix.MultiplyPoint(transform.position), out pathPosition, out pathProgress);
                                targetLookPos = PathContainer.transform.localToWorldMatrix.MultiplyPoint(pathPosition);
                                if (Vector3.Distance(targetLookPos, transform.position) < 1f) onPath = true;
                        }
                }
                targetRotation = Quaternion.LookRotation((targetLookPos - transform.position).normalized);

                Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, _delta * rotationSpeed);
                Player_StaticFunctions.SetTransformRotation(transform, newRotation, "Aerial Movement");

                Rb.linearVelocity = movementSpeed * Mathf.Max(0f, 1f - Quaternion.Angle(targetRotation, transform.rotation) * 0.02f) * transform.forward;
        }
}
