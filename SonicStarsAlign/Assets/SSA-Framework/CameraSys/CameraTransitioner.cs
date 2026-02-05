using UnityEngine;
using UnityEngine.Events;

public class CameraTransitioner : MonoBehaviour
{
        public CamBrain PlayerCameraBrain;

        [Space]
        public CameraTransition Enter;

        public UnityEvent EnterEvent;

        public CameraTransition Exit;
        public UnityEvent ExitEvent;

        private Panel_Collider _triggercl;

        public void OnEnable () {
                _triggercl = GetComponent<Panel_Collider>();
                _triggercl.TriggerEnter += TEnter;
                _triggercl.TriggerExit += TExit;
        }

        public void OnDisable () {
                _triggercl.TriggerEnter -= TEnter;
                _triggercl.TriggerExit -= TExit;
        }

        private void TEnter ( Collider _other ) {
                Debug.Log(_other);
                if (Enter.Point == null) return;

                if (!PlayerCameraBrain)
                {
                        if(_other.gameObject.TryGetComponent(out Sonic_ReferenceObjects References))
                                PlayerCameraBrain = References.CameraBrain;
                }

                var _camPoint = Enter.Point.GetComponent<ICamPoint>();
                PlayerCameraBrain.WeightCurve = Enter.WeightCurve;
                PlayerCameraBrain.Point = _camPoint;
                PlayerCameraBrain.MachineTransition(CameraStates.Transitioning);

                EnterEvent.Invoke();
        }

        private void TExit ( Collider _other ) {
                if (Exit.Point == null) return;

                var _camPoint = Exit.Point.GetComponent<ICamPoint>();
                PlayerCameraBrain.WeightCurve = Exit.WeightCurve;
                PlayerCameraBrain.Point = _camPoint;
                PlayerCameraBrain.MachineTransition(CameraStates.Transitioning);

                ExitEvent.Invoke();
        }
}

[System.Serializable]
public struct CameraTransition
{
        public GameObject Point;
        public AnimationCurve WeightCurve;
}