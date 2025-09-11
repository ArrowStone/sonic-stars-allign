using UnityEngine;

// Script for loading and saving settigns.
public class SettingsManager : MonoBehaviour
{
    public SettingsElement[] settingsElements;

    void Awake()
    {
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
}