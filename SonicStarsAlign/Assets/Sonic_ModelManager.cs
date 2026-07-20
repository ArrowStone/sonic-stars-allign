using UnityEngine;
using System.Collections;

public class Sonic_ModelManager : MonoBehaviour
{

    [SerializeField] Sonic_PlayerStateMachine _CTX;

    [SerializeField] GameObject[] ModelsWhenInBall;
    [SerializeField] GameObject[] ModelsWhenSpinDashing;
    [SerializeField] GameObject[] ModelsWhenNotInBall;

    [SerializeField] Animator NormalAnimator;
    [SerializeField] Animator BallAnimator;

    public float ballRollSpeed;

    public bool InBall { get; private set; } = true;

    private void Start()
    {
        ExitBall();
    }

    private void FixedUpdate()
    {
        if (InBall)
        {
            BallAnimator.SetFloat("Speed", ballRollSpeed);
        }
    }

    public void EnterBall()
    {
        if (InBall) { return; }
        //_CTX.Anim.SetInteger("State", 1);

        InBall = true;

        foreach (var model in ModelsWhenInBall)
        {
            model.SetActive(true);
        }

        foreach (var model in ModelsWhenNotInBall)
        {
            model.SetActive(false);
        }

        foreach (var model in ModelsWhenSpinDashing)
        {
            model.SetActive(false);
        }
    }

    public void ExitBall()
    {
        if (!InBall) { return; }

        InBall = false;

        foreach (var model in ModelsWhenInBall)
        {
            model.SetActive(false);
        }

        foreach (var model in ModelsWhenNotInBall)
        {
            model.SetActive(true);
        }

        foreach (var model in ModelsWhenSpinDashing)
        {
            model.SetActive(false);
        }
    }

    public void EnterSpindash()
    {
        InBall = false;

        foreach (var model in ModelsWhenInBall)
        {
            model.SetActive(false);
        }

        foreach (var model in ModelsWhenNotInBall)
        {
            model.SetActive(false);
        }

        foreach (var model in ModelsWhenSpinDashing)
        {
            model.SetActive(true);
        }
    }
}
