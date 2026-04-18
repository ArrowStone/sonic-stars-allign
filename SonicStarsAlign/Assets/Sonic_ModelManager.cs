using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class Sonic_ModelManager : MonoBehaviour
{

        [SerializeField] Sonic_PlayerStateMachine _CTX;
        
        [SerializeField] GameObject[] ModelsWhenInBall;
        [SerializeField] GameObject[] ModelsWhenNotInBall;

        [SerializeField] Animator NormalAnimator;
        [SerializeField] Animator BallAnimator;

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

        public void EnterBall () {
                if (inBall) { return; }

                inBall = true;

                foreach (var model in ModelsWhenInBall)
                {
                        model.SetActive(true);
                }

                foreach (var model in ModelsWhenNotInBall)
                {
                        model.SetActive(false);
                }
        }

        public void ExitBall () {
                if (!inBall) { return; }

                inBall = false;

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
