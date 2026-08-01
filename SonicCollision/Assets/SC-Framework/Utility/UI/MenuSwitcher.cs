using UnityEngine;
using UnityEngine.SceneManagement;

// Used to exit the menus in the main menu which are separate scenes
public class MenuSwitcher : MonoBehaviour
{
    [SerializeField] int MenuToReturnTo = 0;
    [SerializeField] bool UnloadScene = false;
    [SerializeField] bool ActivateOnStart = false;
    public void Exit()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>())
        {
            if (canvas.gameObject.scene.buildIndex != gameObject.scene.buildIndex)
            {
                canvas.GetComponent<Animator>().SetInteger("Menu", MenuToReturnTo);
                if (UnloadScene) SceneManager.UnloadSceneAsync(gameObject.scene.buildIndex);
                return;
            }
        }
    }

    public void Awake()
    {
        if (ActivateOnStart) Exit();
    }
}
