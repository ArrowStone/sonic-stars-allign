using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// HAndles some aspects of exiting and entering the settings UI from the pause menu
public class SettingsMenuHelper : MonoBehaviour
{
    [SerializeField] Button BackButton;
    [SerializeField] GameObject MenuFirstSelectedObject;
    private InputComponent input;

    void Awake()
    {
        input = FindAnyObjectByType<InputComponent>();
        FindAnyObjectByType<EventSystem>().SetSelectedGameObject(MenuFirstSelectedObject);
    }

    void Update()
    {
        if (input && input.PauseInput.WasPressedThisFrame())
        {
            BackButton.onClick.Invoke();
        }
    }
}
