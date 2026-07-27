using UnityEngine;

// Spawns an object, useful for buttons.
public class Spawner : MonoBehaviour
{
    public void Spawn(GameObject _object)
    {
        Instantiate(_object, transform.position, transform.rotation);
    }
}
