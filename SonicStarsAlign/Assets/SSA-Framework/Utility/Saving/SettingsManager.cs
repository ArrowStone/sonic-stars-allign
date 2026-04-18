using UnityEngine;

// Script for loading and saving settigns.
public class SettingsManager : MonoBehaviour
{
    SettingsElement[] settingsElements;

    void Awake()
    {
        settingsElements = FindObjectsByType<SettingsElement>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        ResetElements();
    }

    public void SaveSetings()
    {
        foreach (SettingsElement element in settingsElements)
        {
            element.SaveValue();
        }
    }

    public void ResetElements()
    {
        foreach (SettingsElement element in settingsElements)
        {
            element.ResetElement();
        }
    }

    public void SetDefaults()
    {
        foreach (SettingsElement element in settingsElements)
        {
            element.SetDefaultValue();
        }
    }
}