using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class ObjectPropertiesProcessor
{
    public string ComponentType;
    public void Process(GameObject gameObject, JObject jObject)
    {
        // Feel free to make this mess clearer if you know how
        Debug.Log(gameObject.name);
        System.Type compType = typeof(ObjectPropertiesProcessor).Assembly.GetType(ComponentType);
        Debug.Log(compType);
        dynamic comp = gameObject.GetComponent(compType);
        Debug.Log(comp);

        Dictionary<string, JToken> paramDictionary = jObject["parameters"].ToObject<Dictionary<string, JToken>>();

        foreach(string key in paramDictionary.Keys)
        {
            Debug.Log(key);
            // I have no idea why the short way doesn't work
            System.Reflection.FieldInfo compField = null;// = compType.GetProperty(key);
            foreach(System.Reflection.FieldInfo info in compType.GetFields())
            {
                Debug.Log(info.Name);
                if(info.Name.Equals(key))
                {
                    Debug.Log("Yeah!");
                    compField = info;
                    break;
                }
            }
            if(compField == null) {Debug.Log("Uh oh " + comp.GetType().FullName); continue;}
            compField.SetValue(comp, paramDictionary[key].ToObject(compField.FieldType));
        }
    }
}