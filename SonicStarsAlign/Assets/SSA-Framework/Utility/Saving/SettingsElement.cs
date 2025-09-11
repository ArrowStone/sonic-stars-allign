using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Basically all settings saving is done here actually. SaveManager basically just provides functions for the buttons to quickly make all the elements
// save or load their values.
public class SettingsElement : MonoBehaviour
{
    public Component component;
    public string saveParamName;

    public void SaveValue()
    {
        switch (component.GetType().Name)
        {
            case "Scrollbar":
                Debug.Log("Scrollbar");
                Scrollbar scrollbar = (Scrollbar)component;
                PlayerPrefs.SetFloat(saveParamName, scrollbar.value);
                break;
            case "TMP_Dropdown":
                Debug.Log("Dropdown");
                TMP_Dropdown dropdown = (TMP_Dropdown)component;
                PlayerPrefs.SetInt(saveParamName, dropdown.value);
                break;
        }
    }

    public void ResetElement()
    {
        switch (component.GetType().Name)
        {
            case "Scrollbar":
                Debug.Log("Scrollbar");
                Scrollbar scrollbar = (Scrollbar)component;
                scrollbar.value = PlayerPrefs.GetFloat(saveParamName);
                break;
            case "TMP_Dropdown":
                Debug.Log("Dropdown");
                TMP_Dropdown dropdown = (TMP_Dropdown)component;
                dropdown.value = PlayerPrefs.GetInt(saveParamName);
                break;
        }
    }
}