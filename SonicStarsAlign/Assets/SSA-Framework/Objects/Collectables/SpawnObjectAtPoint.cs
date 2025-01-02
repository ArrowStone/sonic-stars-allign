using UnityEngine;

public class SpawnObjectAtPoint : MonoBehaviour
{
    public bool Local;
    public Vector3 Offset;

    [Space]
    public bool Rocal;

    public Vector3 RotOffset;

    [Space]
    [SerializeField] private bool hasParent;

    public Transform Parent;
    public GameObject spawnObject;

    public void Spawn()
    {
        if (hasParent)
        {
            _ = Instantiate(spawnObject, Position(), Rotation(), Parent);
        }
        else
        {
            _ = Instantiate(spawnObject, Position(), Rotation());
        }
    }

    private Vector3 Position()
    {
        return Local ? Offset + transform.position : Offset;
    }

    private Quaternion Rotation()
    {
        Quaternion rot = new()
        {
            eulerAngles = RotOffset
        };
        return Rocal ? rot * transform.rotation : rot;
    }
}