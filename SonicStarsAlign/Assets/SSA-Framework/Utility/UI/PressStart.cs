using System.Collections;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.InputSystem;

// The title screen functionality
public class PressStart : MonoBehaviour
{
    public InputComponent input;
    public Animator animator;

    private void Start()
    {
        Debug.Log("Start!");
    }

    // Activated by the animation
    public void WaitForStart()
    {
        Debug.Log("Awake!");
        IEnumerator PressStart()
        {
            Debug.Log("Coroutine!");
            yield return new WaitUntil(() => input.StartInput.IsPressed() || input.Touch.IsPressed());
            animator.SetTrigger("StartPressed");
            Debug.Log("Pressed!");
        }

        StartCoroutine(PressStart());
    }
}
