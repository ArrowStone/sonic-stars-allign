using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Handles switching between tabs in the settings menu
public class TabManager : MonoBehaviour
{
    [SerializeField] Animator[] TabAnims;
    [SerializeField] Animator TabSwithAnim;
    [SerializeField] Transform[] TabObjects;
    private InputComponent input;
    private EventSystem eventSystem;

    private int _tab = 0;

    void Awake()
    {
        input = GetComponent<InputComponent>();
        eventSystem = FindAnyObjectByType<EventSystem>();
        SwitchTab(_tab);
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
    }

    void Update()
    {
        // You have the right to have a differing opinion on whether this is a good way to do this
        bool selectedAnObjectInATab = eventSystem.currentSelectedGameObject &&
                                      eventSystem.currentSelectedGameObject.transform.parent &&
                                      eventSystem.currentSelectedGameObject
                                      .transform.parent.parent.name.Contains("Tab");
        if (input.TabLeftInput.WasPerformedThisFrame() && selectedAnObjectInATab) MoveTab(-1);
        else if (input.TabRightInput.WasPressedThisFrame() && selectedAnObjectInATab) MoveTab(1);
    }

    void MoveTab(int increment)
    {
        _tab += increment;
        _tab = Mathf.Clamp(_tab, 0, 2);
        SwitchTab(_tab);
    }

    // Wrapper to use with UnityEvents
    public void SwitchTab(int tab)
    {
        _tab = tab;
        TabSwithAnim.SetInteger("Tab", tab);
        eventSystem.SetSelectedGameObject(TabObjects[tab].GetChild(0).GetChild(0).gameObject);

        int i = 0;
        foreach (Animator anim in TabAnims)
        {
            anim.SetBool("IsOn", i == tab);
            i += 1;
        }
    }

    // For navigation from the buttons on the bottom
    public void SetSelectedToLastTabObject()
    {
        IEnumerator WaitForSelectionToFinish()
        {
            yield return new WaitForSecondsRealtime(0.05f);

            // You have the right to have a differing opinion on whether this is a good way to do this
            eventSystem.SetSelectedGameObject(TabObjects[_tab].GetChild(TabObjects[_tab].childCount - 1).GetChild(0).gameObject);
        }
        StartCoroutine(WaitForSelectionToFinish());
    }

}
