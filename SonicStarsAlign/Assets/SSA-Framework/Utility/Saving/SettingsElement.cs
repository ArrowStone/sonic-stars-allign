using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Basically all settings saving is done here actually. SaveManager basically just provides functions for the buttons to quickly make all the elements
// save or load their values.
public class SettingsElement : MonoBehaviour
{
    public Component component;
    public string saveParamName;
    public float defaultValue = 0;

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
            case "Toggle":
                Debug.Log("Toggle");
                Toggle toggle = (Toggle)component;
                PlayerPrefs.SetInt(saveParamName, toggle.isOn ? 1 : 0);
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
                scrollbar.value = PlayerPrefs.HasKey(saveParamName) ? PlayerPrefs.GetFloat(saveParamName) : defaultValue;
                break;
            case "TMP_Dropdown":
                Debug.Log("Dropdown");
                TMP_Dropdown dropdown = (TMP_Dropdown)component;
                dropdown.value = PlayerPrefs.HasKey(saveParamName) ? PlayerPrefs.GetInt(saveParamName) : (int)defaultValue;
                break;
            case "Toggle":
                Debug.Log("Toggle");
                Toggle toggle = (Toggle)component;
                toggle.isOn = (PlayerPrefs.HasKey(saveParamName) ? PlayerPrefs.GetInt(saveParamName) : defaultValue) > 0;
                break;
        }
    }

    public void SetDefaultValue()
    {
        switch (component.GetType().Name)
        {
            case "Scrollbar":
                Debug.Log("Scrollbar");
                Scrollbar scrollbar = (Scrollbar)component;
                scrollbar.value = defaultValue;
                break;
            case "TMP_Dropdown":
                Debug.Log("Dropdown");
                TMP_Dropdown dropdown = (TMP_Dropdown)component;
                dropdown.value = (int)defaultValue;
                break;
            case "Toggle":
                Debug.Log("Toggle");
                Toggle toggle = (Toggle)component;
                toggle.isOn = defaultValue > 0;
                break;
        }
    }
}