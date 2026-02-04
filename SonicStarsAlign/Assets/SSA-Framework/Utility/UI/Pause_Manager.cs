using UnityEngine;
using UnityEngine.InputSystem;

// da thing that does pause
public class Pause_Manager : MonoBehaviour
{
    public GameObject pauseMenu;
    public MonoBehaviour[] gameComponents;
    public bool paused = false;
    private InputComponent input;
    private bool wasPauseButtonReleased = true;
    void OnEnable()
    {
        SetPauseState(paused);

        input = GameObject.Find("Player_Rigidbody").GetComponent<Sonic_PlayerStateMachine>().Input;
    }

    public void SetPauseState(bool state)
    {
        Debug.Log("Pause state: " + state.ToString());
        paused = state;
        pauseMenu.SetActive(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        Time.timeScale = state ? 0 : 1;

        foreach (MonoBehaviour comp in gameComponents)
        {
            comp.enabled = !state;
        }

        InputSystem.settings.updateMode = state ? InputSettings.UpdateMode.ProcessEventsInDynamicUpdate : InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
    }

    public void SwitchPauseState()
    {
        SetPauseState(!paused);
    }

    void Update()
    {
        if (input.StartInput.WasPressedThisFrame() && wasPauseButtonReleased)
        {
            wasPauseButtonReleased = false;
            SwitchPauseState();
        }
        else if (input.StartInput.WasReleasedThisFrame())
        {
            wasPauseButtonReleased = true;
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
