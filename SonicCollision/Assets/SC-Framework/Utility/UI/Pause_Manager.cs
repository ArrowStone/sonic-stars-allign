using UnityEngine;
using UnityEngine.InputSystem;

// da thing that does pause
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject PauseMenu;
    public static bool paused = false;
    private InputComponent input;
    private bool wasPauseButtonReleased = true;
    [SerializeField] bool TogglePauseMenuActive;
    [SerializeField] Animator PauseAnimator;
    [SerializeField] Animator[] ButtonAnimators;

    void OnEnable()
    {
        Instance = this;

        SetPauseState(paused);

        input = GameObject.Find("Player_Rigidbody").GetComponent<Sonic_PlayerStateMachine>().Input;
    }

    public void SetPauseState(bool state)
    {

        paused = state;

        if (TogglePauseMenuActive)
        {
            PauseMenu.SetActive(state);
        }
        else
        {
            PauseAnimator.SetBool("Paused", state);
        }

        /*if (!state)
        {
            foreach (Animator anim in ButtonAnimators) { anim.SetTrigger("Off"); }
        }*/

        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        Time.timeScale = state ? 0 : 1;

        AudioListener.pause = state;
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
