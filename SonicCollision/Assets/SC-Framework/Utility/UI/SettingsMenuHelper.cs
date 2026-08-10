using UnityEngine;
using UnityEngine.UI;

// Handles some aspects of exiting and entering the settings UI from the pause menu
public class SettingsMenuHelper : MonoBehaviour
{
    [SerializeField] AudioSource[] AudioSourcesToKeepPlaying;
    [SerializeField] Button BackButton;
    private InputComponent input;

    void Awake()
    {
        input = FindAnyObjectByType<InputComponent>();

        foreach (AudioSource source in AudioSourcesToKeepPlaying)
        {
            source.ignoreListenerPause = true; // I do wish this would be a setting in the component
        }
    }

    void Update()
    {
        if (input && input.PauseInput.WasPressedThisFrame())
        {
            BackButton.onClick.Invoke();
        }
    }

    public void ApplySettings()
    {
        FindAnyObjectByType<SettingsLoader>()?.ApplySettings();
    }
}
