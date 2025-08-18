using UnityEngine;

// da thing that does pause
public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public MonoBehaviour[] gameComponents;
    public bool paused = false;

    void OnEnable()
    {
        SetPauseState(paused);
    }

    public void SetPauseState(bool state)
    {
        paused = state;
        pauseMenu.SetActive(state);
        Cursor.lockState = (state) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        Time.timeScale = (state) ? 0 : 1;

        foreach (MonoBehaviour comp in gameComponents)
        {
            comp.enabled = !state;
        }
    }

    public void SwitchPauseState()
    {
        SetPauseState(!paused);
    }

    // TODO: Figure out how to use the proper input system and stuff
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPauseState();
        }
    }

    public void ExitGame()
    {
        // Credit: Unity Docs
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
