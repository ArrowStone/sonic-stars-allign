using System.Collections;
using UnityEngine;

// The title screen functionality
public class PressStart : MonoBehaviour
{
    public InputComponent input;
    public Animator animator;

    public void WaitForStart()
    {
        Debug.Log("Awake!");
        IEnumerator PressStart()
        {
            Debug.Log("Coroutine!");
            yield return new WaitUntil(() => input.StartInput.IsPressed());
            animator.SetTrigger("StartPressed");
            Debug.Log("Pressed!");
        }

        StartCoroutine(PressStart());
    }
}
