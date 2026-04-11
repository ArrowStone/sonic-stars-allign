using UnityEngine;

// Mainly to switch between menus in the main menu scene using anims.
public class AnimatorSetForwarder : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private string menuProperty;

    public void SetMenu(int value)
    {
        anim.SetInteger(menuProperty, value);
    }
}
