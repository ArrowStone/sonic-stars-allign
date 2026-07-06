using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeviceDependentActivation : MonoBehaviour
{
    public string DesiredDevice;
    public UnityEvent PresentEvent;
    public UnityEvent AbsentEvent;
    void Awake()
    {
        #if UNITY_ANDROID || UNITY_EDITOR
        IEnumerator ConnectionWait()
        {
            #if UNITY_EDITOR
            // Takes a moment for Unity Remote to connect
            yield return new WaitForSeconds(0.2f);
            #endif
            bool setActive = false;
            foreach (InputDevice device in InputSystem.devices)
            {
                if(device.name == DesiredDevice)
                {
                    setActive = true;
                }
            }
            if(setActive) PresentEvent.Invoke();
            else AbsentEvent.Invoke();
        }
        StartCoroutine(ConnectionWait());
        #else
        AbsentEvent.Invoke();
        #endif
    }
}
