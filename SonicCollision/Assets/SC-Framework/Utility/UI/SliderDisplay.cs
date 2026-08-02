using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Used to display the percentage values and such for sliders in UI
// (Just in the settings for now)
public class SliderDisplay : MonoBehaviour
{
    [SerializeField] string DisplayFormat = "{0}%";
    [SerializeField] TMP_Text TextElement;
    [SerializeField] Slider Slider;

    void Start()
    {
        TextElement = TextElement ? TextElement : GetComponent<TMP_Text>();
        Slider = Slider ? Slider : GetComponent<Slider>();
    }

    public void SetValue()
    {
        TextElement.text = string.Format(DisplayFormat, Math.Floor(Slider.value * 100f));
    }
}
