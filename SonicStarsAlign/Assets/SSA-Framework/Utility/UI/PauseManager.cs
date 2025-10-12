using UnityEngine;

// da thing that does pause
public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public MonoBehaviour[] gameComponents;
    public bool paused = false;
    InputComponent input;
    void OnEnable()
    {
        SetPauseState(paused);

        input = GameObject.Find("Player_Rigidbody").GetComponent<Sonic_PlayerStateMachine>().Input;
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

    void FixedUpdate()
    {
        if (input.StartInput.WasPressedThisFrame())
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
