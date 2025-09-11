using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The functionality of switching between several values using a slider
public class SliderSwitch : MonoBehaviour
{
    public Scrollbar scrollbar;
    public TMP_Text text;
    public string[] values = {
        "25",
        "50",
        "75",
        "100"
    };

    public void SetValue()
    {
        text.text = values[(int) (scrollbar.value * (values.Length-1))];
    }
}
