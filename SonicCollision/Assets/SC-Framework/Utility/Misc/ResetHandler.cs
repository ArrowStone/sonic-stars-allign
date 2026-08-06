using UnityEngine;

// Handles resetting every individual object on stage restart or respawn
public class ResetHandler : MonoBehaviour
{
    public bool doResetPosition;
    private Rigidbody rb;
    private Enemy_MovementStateMachine etx;
    private PosRot startTransform;
    private PosRot checkTransform; // Check is short for checkpoint
    private Vector3 checkLinearVelocity;
    private Vector3 checkAngularVelocity;
    private bool isCollectable;
    private bool checkCollected = false;

    void Awake()
    {
        if (doResetPosition)
        {
            startTransform = new()
            {
                Position = transform.position,
                Rotation = transform.rotation
            };
            rb = GetComponent<Rigidbody>();
        }
        isCollectable = TryGetComponent<CollectableBase>(out _);
        etx = GetComponent<Enemy_MovementStateMachine>();
        RecordCheckpoint();
    }

    public void Reset()
    {
        if (doResetPosition)
        {
            rb.MovePosition(startTransform.Position);
            rb.MoveRotation(startTransform.Rotation);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (isCollectable)
        {
            gameObject.SetActive(true);
        }
        etx?.Start();
    }

    public void RecordCheckpoint()
    {
        if (doResetPosition)
        {
            checkTransform = new()
            {
                Position = transform.position,
                Rotation = transform.rotation
            };
            if (etx)
            {
                // Rambus go vroom vroom
                checkLinearVelocity = etx.VerticalVelocity;
                checkAngularVelocity = etx.HorizontalVelocity;
            }
            else
            {
                checkLinearVelocity = rb.linearVelocity;
                checkAngularVelocity = rb.angularVelocity;
            }
        }
        if (isCollectable)
        {
            checkCollected = gameObject.activeInHierarchy;
        }
    }

    public void ResetToCheckpoint()
    {
        if (doResetPosition)
        {
            rb.MovePosition(checkTransform.Position);
            rb.MoveRotation(checkTransform.Rotation);
            if (etx)
            {
                etx.HorizontalVelocity = checkLinearVelocity;
                etx.VerticalVelocity = checkAngularVelocity;
            }
            else
            {
                rb.linearVelocity = checkLinearVelocity;
                rb.angularVelocity = checkAngularVelocity;
            }
        }
        if (isCollectable)
        {
            gameObject.SetActive(checkCollected);
        }
    }
}
