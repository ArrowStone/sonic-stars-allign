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

                //Get Point (Camera type) to set to from public Enter fields, and apply.
                var _camPoint = Enter.Point.GetComponent<ICamPointStyle>();
                PlayerCameraBrain.WeightCurve = Enter.WeightCurve;

                SetPointAndTransitionState(_camPoint);

                EnterEvent.Invoke();
        }

        private void TExit ( Collider _other ) {
                if (Exit.Point == null) return;

                //Get Point (Camera type) to set to from public Exit fields, and apply.
                ICamPointStyle _camPoint = Exit.Point.GetComponent<ICamPointStyle>();
                PlayerCameraBrain.WeightCurve = Exit.WeightCurve;

                SetPointAndTransitionState(_camPoint);

                ExitEvent.Invoke();
        }

        private void SetPointAndTransitionState ( ICamPointStyle _camPoint ) {
                //Set point first because OnEnterPoint will be called in when Transitioning State is entered.
                PlayerCameraBrain.Point.OnExitPoint();
                PlayerCameraBrain.Point = _camPoint;
                PlayerCameraBrain.MachineTransition(CameraStates.Transitioning);
        }
}

[System.Serializable]
public struct CameraTransition
{
        public GameObject Point;
        public AnimationCurve WeightCurve;
}