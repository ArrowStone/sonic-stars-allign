using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class Sonic_ModelManager : MonoBehaviour
{

        [SerializeField] Sonic_PlayerStateMachine _CTX;
        
        [SerializeField] GameObject[] ModelsWhenInBall;
        [SerializeField] GameObject[] ModelsWhenSpinDashing;
        [SerializeField] GameObject[] ModelsWhenNotInBall;

        [SerializeField] Animator NormalAnimator;
        [SerializeField] Animator BallAnimator;
        private Coroutine ballCoroutine;

        public float ballRollSpeed;

        private bool inBall = true;

        private void Start () {
                ExitBall();
        }

        private void FixedUpdate () 
        {
                if (inBall)
                {
                        BallAnimator.SetFloat("Speed", ballRollSpeed);
                }
        }

        public void EnterBall (bool waitForAnim = false) {
                if (inBall) { return; }
                _CTX.Anim.SetInteger("State", 1);

                inBall = true;
                if(ballCoroutine != null)
                        _CTX.StopCoroutine(ballCoroutine);

                void SwitchModels()
                {
                        foreach (var model in ModelsWhenInBall)
                        {
                                model.SetActive(true);
                        }

                        foreach (var model in ModelsWhenNotInBall)
                        {
                                model.SetActive(false);
                        } 
                }

                if(!waitForAnim)
                {
                        SwitchModels();
                        return;
                }

                IEnumerator WaitForAnimEnd()
                {
                        yield return new WaitForSeconds(0.1f);
                        SwitchModels();
                }
                ballCoroutine = _CTX.StartCoroutine(WaitForAnimEnd());
        }

        public void ExitBall () {
                if (!inBall) { return; }

                inBall = false;

                if(ballCoroutine != null)
                        _CTX.StopCoroutine(ballCoroutine);

                foreach (var model in ModelsWhenInBall)
                {
                        model.SetActive(false);
                }

                foreach (var model in ModelsWhenNotInBall)
                {
                        model.SetActive(true);
                }
        }
}
