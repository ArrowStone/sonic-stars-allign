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


        private bool inBall = true;

        private void Start () {
                ExitBall();
        }

        private void Update () {
 
                if ( inBall && _CTX.CurrentEstate is PlayerStates.Air)
                {
                        BallAnimator.SetFloat("Speed", 40);
                }
                else if (inBall)
                {
                        Debug.Log(_CTX.PlayerRunningSpeed);
                        BallAnimator.SetFloat("Speed", _CTX.PlayerRunningSpeed);
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
