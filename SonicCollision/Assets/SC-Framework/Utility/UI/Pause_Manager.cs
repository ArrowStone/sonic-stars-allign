using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// da thing that does pause
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject PauseMenu;
    public static bool paused = false;
    private InputComponent input;
    private bool wasPauseButtonReleased = true;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] bool TogglePauseMenuActive;
    [SerializeField] Animator PauseAnimator;
    [SerializeField] Animator[] ButtonAnimators;
    [SerializeField] GameObject ObjectToSelectOnPause;
    [SerializeField] Button[] PauseButtons;
    void OnEnable()
    {
        Instance = this;

        SetPauseState(paused);

        input = GameObject.Find("Player_Rigidbody").GetComponent<Sonic_PlayerStateMachine>().Input;
    }

    public void SetPauseState(bool state)
    {
        Debug.Log("Set pause: " + state);

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

        foreach (Button b in PauseButtons)
        {
            b.interactable = state;
        }
        //eventSystem.enabled = state;

        if (state) SetSelectedObject();
    }

    public void SetSelectedObject()
    {
        Debug.Log("Setting!");
        eventSystem.SetSelectedGameObject(ObjectToSelectOnPause);
    }

    public void SwitchPauseState()
    {
        SetPauseState(!paused);
    }

    void Update()
    {
        if (input.PauseInput.WasPressedThisFrame() && wasPauseButtonReleased
            && SceneManager.sceneCount == 1) // Checking for settings UI
        {
            wasPauseButtonReleased = false;
            SwitchPauseState();
        }
        else if (input.PauseInput.WasReleasedThisFrame())
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
