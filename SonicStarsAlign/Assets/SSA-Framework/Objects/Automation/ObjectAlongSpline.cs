using UnityEngine;
using UnityEngine.Splines;

public class ObjectAlongSpline : MonoBehaviour
{
    public GameObject Object;
    public float Count;
    public SplineContainer Spline;

    [ContextMenu("Regenerate")]
    public void Along()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (float i = 0; i < 1; i += 1 / Count)
        {
            Vector3 pos = Spline.EvaluatePosition(i);
            _ = Instantiate(Object, pos, Quaternion.identity, transform);
        }
    }
}