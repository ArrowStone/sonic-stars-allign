using UnityEngine;
using System.IO;

// Handles writing the save data to a pernament storage as well as retrieving data.
public static class DataSaver
{
    public static string GetSaveFileForStage(StageData data)
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "save-0/");
        string saveFile = Path.Combine(saveFolder, data.saveFile + ".json");
        Debug.Log("Accessing " + saveFile);
        return saveFile;
    }
    public static void RecordStageData(StageData data)
    {
        string saveData = JsonUtility.ToJson(data);
        string filePath = GetSaveFileForStage(data);
        if(File.Exists(filePath))
        {
            File.WriteAllText(filePath, saveData, System.Text.Encoding.UTF8);
        }
        else
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "save-0/"));
            Debug.Log("Creating new save file!");
            StreamWriter file = File.CreateText(filePath);
            file.Write(saveData);
            file.Close();
        }
    }

    public static void ReadStageData(StageData data)
    {
        string filePath = GetSaveFileForStage(data);
        if(File.Exists(filePath))
        {
            string textData = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            JsonUtility.FromJsonOverwrite(textData, data);
        }
    }
}
