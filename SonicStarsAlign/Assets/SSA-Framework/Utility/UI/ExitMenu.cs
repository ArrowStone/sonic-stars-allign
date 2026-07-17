using UnityEngine;

// Used to exit the menus in the main menu which are separate scenes
public class ExitMenu : MonoBehaviour
{
    public void Exit()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>())
        {
            if (canvas.gameObject.scene.buildIndex != gameObject.scene.buildIndex)
            {
                canvas.GetComponent<Animator>().SetInteger("Menu", 0);
                return;
            }
        }
    }
}
