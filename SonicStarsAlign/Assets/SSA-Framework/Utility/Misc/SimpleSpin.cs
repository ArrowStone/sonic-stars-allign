using UnityEngine;

public class SimpleSpin : MonoBehaviour
{
    [SerializeField] Vector3 RotationIncrement;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(RotationIncrement);
    }
}
