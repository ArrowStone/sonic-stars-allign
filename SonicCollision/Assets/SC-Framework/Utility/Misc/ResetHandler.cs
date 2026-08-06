using UnityEngine;
using UnityEngine.Events;

public class ResetHandler : MonoBehaviour
{
    public UnityEvent ResetEvent;
    public bool doResetPosition;
    private Rigidbody rb;
    private Vector3 startingPos;
    private Quaternion startingRot;

    void Awake()
    {
        if (doResetPosition)
        {
            startingPos = transform.position;
            startingRot = transform.rotation;
            rb = GetComponent<Rigidbody>();
        }
    }

    public void Reset()
    {
        ResetEvent.Invoke();
        if (doResetPosition)
        {
            rb.MovePosition(startingPos);
            rb.MoveRotation(startingRot);
        }
    }
}
