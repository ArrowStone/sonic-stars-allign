using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

[System.Serializable]
public class LinkedLevelObject
{
    public string ObjectID;
    public GameObject LevelObject;
    public string ComponentType;
    public void ProcessProperties(GameObject gameObject, JObject jObject)
    {
        // Feel free to make this mess clearer if you know how
        if(ComponentType.Equals("")) return;

        System.Type compType = typeof(LinkedLevelObject).Assembly.GetType(ComponentType);

        if(compType == null)
        {
            Debug.LogWarning(string.Format("Could not find component type for {0}", gameObject.name));
            return;
        }

        dynamic comp = gameObject.GetComponent(compType);
        if(comp == null)
        {
            Debug.LogWarning(string.Format("Could not find {0} for {1}", ComponentType, gameObject.name));
            return;
        }

        Dictionary<string, JToken> paramDictionary = jObject["parameters"].ToObject<Dictionary<string, JToken>>();

        foreach(string key in paramDictionary.Keys)
        {
            System.Reflection.FieldInfo compField = compType.GetField(key);
            if(compField == null)   continue;
            compField.SetValue(comp, paramDictionary[key].ToObject(compField.FieldType));
        }
    }
}

// Places level geometry & parses the level object JSON on scene load
public class GameLevelLoader : MonoBehaviour
{
    [SerializeField] string sceneAssetPath;
    [SerializeField] List<LinkedLevelObject> levelObjects;
    [SerializeField] int groundLayer;
    void Start()
    {
        Scene levelScene = SceneManager.CreateScene("Level");
        GameObject levelMesh = (GameObject) Instantiate(Resources.Load<GameObject>(sceneAssetPath), levelScene);

        // Add collider and assign the ground layer to each piece of geometry
        foreach(Transform child in levelMesh.transform)
        {
            if (child.TryGetComponent(out MeshFilter _))
            {
                child.gameObject.AddComponent<MeshCollider>();
                child.gameObject.layer = groundLayer;
            }
        }

        // Do same stuff if there's only one object
        if (levelMesh.TryGetComponent(out MeshFilter _))
        {
            levelMesh.gameObject.AddComponent<MeshCollider>();
            levelMesh.gameObject.layer = groundLayer;
        }

        // Load the level object JSON
        TextAsset levelObjectData = Resources.Load<TextAsset>(sceneAssetPath);
        JObject[] objects = JsonConvert.DeserializeObject<JObject[]>(levelObjectData.text);

        foreach(JObject obj in objects)
        {
            // Please don't sue us Nintendo
            LinkedLevelObject link = null;
            string objType = obj["type"].ToString();

            // Get the prefab to place
            foreach (LinkedLevelObject linkedObject in levelObjects)
            {
                if(linkedObject.ObjectID == objType)
                {
                    link = linkedObject;
                    break;
                }
            }
            // No type / No matching prefabs => ignore
            if (link == null) 
            {
                if(objType != null) 
                    Debug.LogWarning(
                        string.Format("Unassigned/invalid object type: {0} ({1})", 
                        objType, obj["name"].ToString()));
                continue;
            }

            // Place the object and assign common parameters
            GameObject gobj = (GameObject) Instantiate(link.LevelObject, levelScene);

            float[] pos = obj["position"].ToObject<float[]>();
            float[] rot = obj["rotation"].ToObject<float[]>();

            gobj.name = obj["name"].ToString();
            gobj.transform.position = new Vector3(-pos[0], pos[2], -pos[1]); // Thanks Unity very cool
            gobj.transform.eulerAngles = new Vector3(rot[0], rot[1], rot[2]) * 360f;

            // Rails and crap
            if(obj.ContainsKey("curve"))
            {
                SplineContainer cont = gobj.GetComponent<SplineContainer>();
                cont = cont ? cont : gobj.GetComponentInChildren<SplineContainer>();
                if(!cont)
                {
                    Debug.LogWarning(string.Format("A curve object ({0}) has been assigned to a non-Spline prefab ({1}).", gobj.name, link.LevelObject.name));
                    continue;
                }

                float[][] curvePoints = obj["curve"].ToObject<float[][]>();

                if(cont.Splines.Count == 0) cont.AddSpline();

                foreach(float[] point in curvePoints)
                {
                    // Have to do the math because Blender deals with handle positions but Unity wants normals
                    float3 pointPos = new float3(-point[0], point[2], -point[1]);
                    float3 inNormal = new float3(-point[3], point[5], -point[4]) - pointPos;
                    float3 outNormal = new float3(-point[6], point[8], -point[7]) - pointPos;
                    cont.Spline.Add(new BezierKnot( pointPos, inNormal, outNormal ));
                }
            }

            // Set properties from Blender
            link.ProcessProperties(gobj, obj);
        }
    }
}
