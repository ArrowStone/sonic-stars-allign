using UnityEngine;

namespace Assets.SEM_Framework.PlayerCharacter
{
    internal class Sonic_AnimationComponent : MonoBehaviour
    {
        public Transform Graphic;
        public Animator Main;

        #region Util

        [SerializeField] private Sonic_PlayerStateMachine _ctx;

        #endregion Util

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void Update()
        {
            GraphicUpdate(Time.deltaTime);
            ParameterUpdate(Time.deltaTime);
        }

        #region AdditionalFunctions

        private void GraphicUpdate(float _delta)
        {
            Graphic.SetPositionAndRotation(_ctx.Rb.position, _ctx.Rb.rotation);
        }

        private void ParameterUpdate(float _delta)
        {
            Main.SetFloat("V_SPD", Vector3.Dot(-_ctx.Gravity.normalized, _ctx.VerticalVelocity));
            Main.SetFloat("H_SPD", Vector3.Dot(_ctx.HorizontalVelocity, _ctx.PlayerDirection));
            Main.SetFloat("SPD", _ctx.Velocity.magnitude);
        }

        #endregion AdditionalFunctions
    }
}