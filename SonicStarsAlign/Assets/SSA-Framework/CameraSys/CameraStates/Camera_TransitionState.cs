using UnityEngine;

public class Camera_TransitionState : IState
{
        private readonly CamBrain _ctx;

        private float _time;
        private float _duration;

        public Camera_TransitionState ( CamBrain _machine ) {
                _ctx = _machine;
        }

        public void EnterState () {

                _time = 0;
                _duration = _ctx.WeightCurve.keys[_ctx.WeightCurve.length - 1].time;

                _ctx.Point.OnEnterPoint(_ctx);
        }

        public void UpdateState () {
                float _delta = Time.deltaTime;
                _time += _delta;

                TransitionMovement(_delta);
                TransitionSwitchConditions();
        }

        public void FixedUpdateState () {
        }

        public void LateUpdateState () {
        }

        public void ExitState () {
                _ctx.CashedTransform = new()
                {
                        Position = _ctx.CamTransform.position,
                        Rotation = _ctx.CamTransform.rotation,
                };
        }

        public void TransitionMovement ( float _delta ) {
                _ctx.Point.ExecutePoint(_delta);
        }

        public void TransitionSwitchConditions () {
                if (_time >= _duration)
                {
                        _ctx.MachineTransition(CameraStates.Alive);
                }
        }
}