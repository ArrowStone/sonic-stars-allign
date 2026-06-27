using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

[AddComponentMenu("Input/Screen Region Delta")]
public class ScreenRegionInput : OnScreenControl, IPointerMoveHandler
{
    public void OnPointerMove(PointerEventData data)
    {
        SendValueToControl(data.delta);
    }

    [InputControl(layout = "Vector 2")]
    [SerializeField]
    private string m_ControlPath;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
}