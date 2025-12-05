using UnityEngine;
using UnityEngine.Splines;

public class ObjectAlongSpline : MonoBehaviour
{
    public GameObject Object;
    public float Count;
    public bool rotateObjects = true;
    public SplineContainer Spline;

    [ContextMenu("Regenerate")]
    public void Along()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        GameObject currentObject = null; // For rotation
        GameObject prevObject;

        for (float i = 0; i < 1; i += 1 / Count)
        {
            prevObject = currentObject;

            Vector3 pos = Spline.EvaluatePosition(i);
            currentObject = Instantiate(Object, pos, Quaternion.identity, transform);

            if (prevObject & rotateObjects)
            {
                Vector3 lookPos = prevObject.transform.position;
                lookPos.y = currentObject.transform.position.y;
                currentObject.transform.LookAt(lookPos);
            }
        }
    }
}