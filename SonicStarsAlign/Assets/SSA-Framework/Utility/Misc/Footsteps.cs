using UnityEngine;

// Produces the footsteps sounds.
public class Footsteps : StateMachineBehaviour
{
    public Sonic_PlayerStateMachine _ctx;
    public float stepNumber = 10f;
    private float lastStepTime = 0f;
    public float firstStepTime = 0f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        lastStepTime = stateInfo.normalizedTime;
        if (!_ctx)
        {
            _ctx = animator.transform.parent.parent.GetComponent<Sonic_PlayerStateMachine>();
        }
    }
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > lastStepTime + firstStepTime)
        {
            lastStepTime += 1 / stepNumber;
            // _ctx.Snd.PlayFootstep();

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
