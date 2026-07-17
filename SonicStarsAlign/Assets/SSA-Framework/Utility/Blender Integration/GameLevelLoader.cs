using System;
using System.Collections.Generic;
using System.IO;
using GLTFast;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

[Serializable]
public class LinkedLevelObject
{
    public string ObjectID;
    public GameObject LevelObject;
    public string ComponentType;
    public void ProcessProperties(GameObject gameObject, JObject jObject)
    {
        // Feel free to make this mess clearer if you know how
        if (ComponentType.Equals("")) return;

        Type compType = typeof(LinkedLevelObject).Assembly.GetType(ComponentType);

        if (compType == null)
        {
            Debug.LogWarning(string.Format("Could not find component type for {0}", gameObject.name));
            return;
        }

        dynamic comp = gameObject.GetComponent(compType);
        if (comp == null)
        {
            Debug.LogWarning(string.Format("Could not find {0} for {1}", ComponentType, gameObject.name));
            return;
        }

        Dictionary<string, JToken> paramDictionary = jObject["parameters"].ToObject<Dictionary<string, JToken>>();

        foreach (string key in paramDictionary.Keys)
        {
            System.Reflection.FieldInfo compField = compType.GetField(key);
            if (compField == null) continue;
            compField.SetValue(comp, paramDictionary[key].ToObject(compField.FieldType));
        }
    }
}

// Places level geometry & parses the level object JSON on scene load
public class GameLevelLoader : MonoBehaviour
{
    private string absolutePath; // Overrides the text asset, used for tesing levels in a standalone player
    [SerializeField] TMP_InputField PathInput;
    [SerializeField] TextAsset LevelData;
    [SerializeField] List<LinkedLevelObject> levelObjects;
    [SerializeField] int groundLayer;

    async void Awake()
    {
        //LoadLevel();
    }

    void Update()
    {
        if (!PathInput) return;
        absolutePath = PathInput.text;
    }

    public void SetLoadPath(string path)
    {
        absolutePath = path;
    }

    public async void LoadLevel()
    {
        Scene existingLevel = SceneManager.GetSceneByName("Level");
        Debug.Log(existingLevel.isLoaded);
        if (existingLevel.isLoaded) await SceneManager.UnloadSceneAsync(existingLevel);

        Scene levelScene = SceneManager.CreateScene("Level");
        GltfImport gltf = new GltfImport();

        string filePath = absolutePath;

        // Unity complains if I don't include the #if.
#if UNITY_EDITOR
        if (absolutePath == "")
            filePath = Application.dataPath.Replace("Assets", "") +
                                AssetDatabase.GetAssetPath(LevelData);
#endif

        string gltfPath = "file://" + filePath.Replace(".json", ".glb");
        bool success = await gltf.Load(gltfPath);
        if (!success)
        {
            // Can't find the .glb file
            Debug.LogError(string.Format("glTF file not found at \"{0}\".", gltfPath));
            return;
        }
        GameObject levelMesh = (GameObject)Instantiate(new GameObject("Level"), levelScene);
        await gltf.InstantiateMainSceneAsync(levelMesh.transform);

        Debug.Log("=== COLLIDERS INCOMING ===");

        // Add collider and assign the ground layer to each piece of geometry
        foreach (Transform child in levelMesh.GetComponentsInChildren<Transform>())
        {
            Debug.Log(child.name);
            if (child.TryGetComponent(out MeshFilter _))
            {
                Debug.Log("Mesh!");
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

        // Handle the level object JSON
        string textData = LevelData.text;
        if (absolutePath != "") textData = File.ReadAllText(absolutePath);
        JObject[] objects = JsonConvert.DeserializeObject<JObject[]>(textData);

        foreach (JObject obj in objects)
        {
            Debug.Log(obj["name"]);
            // Please don't sue us Nintendo
            LinkedLevelObject link = null;
            string objType = obj["type"].ToString();
            Debug.Log(objType);

            // Get the prefab to place
            foreach (LinkedLevelObject linkedObject in levelObjects)
            {
                if (linkedObject.ObjectID == objType)
                {
                    link = linkedObject;
                    break;
                }
            }
            // No type / No matching prefabs => ignore
            if (link == null)
            {
                if (objType != null)
                    Debug.LogWarning(
                        string.Format("Unassigned/invalid object type: {0} ({1})",
                        objType, obj["name"].ToString()));
                continue;
            }

            Debug.Log(link.LevelObject.name);

            // Place the object and assign common parameters
            GameObject gobj = (GameObject)Instantiate(link.LevelObject, levelScene);

            float[] pos = obj["position"].ToObject<float[]>();
            Debug.Log(obj["position"]);
            Debug.Log(string.Format("{0} {1} {2}", pos[0], pos[1], pos[2]));
            float[] rot = obj["rotation"].ToObject<float[]>();

            gobj.name = obj["name"].ToString();
            gobj.transform.position = new Vector3(-pos[0], pos[2], -pos[1]); // Thanks Unity very cool
            gobj.transform.eulerAngles = new Vector3(rot[0], -rot[2], rot[1]) * Mathf.Rad2Deg;

            // Rails and crap
            if (obj.ContainsKey("curve"))
            {
                Debug.Log("Spline processing!");
                SplineContainer cont = gobj.GetComponent<SplineContainer>();
                cont = cont ? cont : gobj.GetComponentInChildren<SplineContainer>();
                if (!cont)
                {
                    Debug.LogWarning(string.Format("A curve object ({0}) has been assigned to a non-Spline prefab ({1}).", gobj.name, link.LevelObject.name));
                    continue;
                }

                float[][] curvePoints = obj["curve"].ToObject<float[][]>();

                if (cont.Splines.Count == 0) cont.AddSpline();

                foreach (float[] point in curvePoints)
                {
                    // Have to do the math because Blender deals with handle positions but Unity wants normals
                    float3 pointPos = new float3(-point[0], point[2], -point[1]);
                    float3 inNormal = new float3(-point[3], point[5], -point[4]) - pointPos;
                    float3 outNormal = new float3(-point[6], point[8], -point[7]) - pointPos;
                    cont.Spline.Add(new BezierKnot(pointPos, inNormal, outNormal));
                }
            }

            // Set properties from Blender
            link.ProcessProperties(gobj, obj);
        }
    }
}
