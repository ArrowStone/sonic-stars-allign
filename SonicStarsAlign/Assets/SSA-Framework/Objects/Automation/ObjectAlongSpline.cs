using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class ObjectAlongSpline : MonoBehaviour
{
    #if UNITY_EDITOR
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
        bool rotatedFirst = false;

        for (float i = 0; i < 1; i += 1 / Count)
        {
            prevObject = currentObject;

            Vector3 pos = Spline.EvaluatePosition(i);
            currentObject = (GameObject) PrefabUtility.InstantiatePrefab(Object, transform);
            currentObject.transform.position = pos;

            if (prevObject & rotateObjects)
            {
                Vector3 lookPos = prevObject.transform.position;
                lookPos.y = currentObject.transform.position.y;
                currentObject.transform.LookAt(lookPos);
            }
            if(rotateObjects && i > 0 && !rotatedFirst)
            {
                rotatedFirst = true;
                Vector3 lookPos = currentObject.transform.position;
                lookPos.y = prevObject.transform.position.y;
                prevObject.transform.LookAt(lookPos);
            }
        }
    }
    #endif
}