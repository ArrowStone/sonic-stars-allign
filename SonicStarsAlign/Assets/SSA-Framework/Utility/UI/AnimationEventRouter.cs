using UnityEngine;
using UnityEngine.Events;

// Basically allows animation to activate events on objects other than the one with the animator.
public class AnimationEventRouter : MonoBehaviour
{
    [SerializeField] UnityEvent[] events;

    public void InvokeEvent(int eventIndex)
    {
        events[eventIndex].Invoke();
    }
}
