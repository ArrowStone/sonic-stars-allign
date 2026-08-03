using UnityEngine;
using UnityEngine.SceneManagement;

// Mainly to switch between menus in the main menu scene using anims.
// Also used by the pause menu to handle some parts of settings menu switching
public class AnimatorSetForwarder : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private string menuProperty;
    private bool waitForSettingsToClose = false;

    public void SetMenu(int value)
    {
        anim.SetInteger(menuProperty, value);
    }
    public void SetMenu(bool value)
    {
        anim.SetBool(menuProperty, value);
    }

    public void SetWaitForSettingsToClose()
    {
        waitForSettingsToClose = true;
    }

    void Update()
    {
        if (waitForSettingsToClose && SceneManager.sceneCount == 1)
        {
            // Settings closed => return pause menu
            SetMenu(true);
            waitForSettingsToClose = false;
        }
    }
}
