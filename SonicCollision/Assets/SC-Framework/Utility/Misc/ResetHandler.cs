using UnityEngine;

// Handles resetting every individual object on stage restart or respawn
public class ResetHandler : MonoBehaviour
{
    public bool doResetPosition;
    private Rigidbody rb;
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
            checkLinearVelocity = rb.linearVelocity;
            checkAngularVelocity = rb.angularVelocity;
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
            rb.linearVelocity = checkLinearVelocity;
            rb.angularVelocity = checkAngularVelocity;
        }
        if (isCollectable)
        {
            gameObject.SetActive(checkCollected);
        }
    }
}
