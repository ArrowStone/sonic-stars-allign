using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Used to exit the menus in the main menu which are separate scenes
public class MenuSwitcher : MonoBehaviour
{
    [SerializeField] int MenuToReturnTo = 0;
    [SerializeField] bool UnloadScene = false;
    [SerializeField] bool ActivateOnStart = false;
    public void Exit(float delay = 0f)
    {
        IEnumerator ExitDelay()
        {
            yield return new WaitForSeconds(delay);
            foreach (Canvas canvas in FindObjectsByType<Canvas>())
            {
                if (canvas.gameObject.scene.buildIndex != gameObject.scene.buildIndex)
                {
                    canvas.GetComponent<Animator>().SetInteger("Menu", MenuToReturnTo);
                    if (UnloadScene) SceneManager.UnloadSceneAsync(gameObject.scene.buildIndex);
                    break;
                }
            }
        }
        StartCoroutine(ExitDelay());
    }

    public void Awake()
    {
        if (ActivateOnStart) Exit();
    }
}
