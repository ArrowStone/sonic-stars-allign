using UnityEngine;

public class Enemy_MovementBase : MonoBehaviour
{
    public Transform target;
    private Rigidbody Rb;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        transform.LookAt(target);
        Rb.linearVelocity = transform.forward;
    }
}
