using UnityEngine;
using System.IO;

// Handles writing the save data to a pernament storage as well as retrieving data.
public static class DataSaving
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

        Debug.Log(string.Format("Saving into file at {0}!", filePath));
        if (File.Exists(filePath))
        {
            File.WriteAllText(filePath, saveData, System.Text.Encoding.UTF8);
        }
        else
        {
            // Intention is to have more save files down the line, so the number 0 will change.
            string saveFolder = Path.Combine(Application.persistentDataPath, "save-0/");
            Directory.CreateDirectory(saveFolder);
            Debug.Log(string.Format("Creating new save file in folder {0}!", saveFolder));
            StreamWriter file = File.CreateText(filePath);
            file.Write(saveData);
            file.Close();
        }
    }

    public static void ReadStageData(StageData data)
    {
        string filePath = GetSaveFileForStage(data);
        if (File.Exists(filePath))
        {
            int[] rankScores = (int[])data.RankScores.Clone();
            int stageRingCount = data.StageRingCount;
            float targetTime = data.TargetTime;

            string textData = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            JsonUtility.FromJsonOverwrite(textData, data);

            data.RankScores = rankScores;
            data.StageRingCount = stageRingCount;
            data.TargetTime = targetTime;
            Debug.Log(data.RedRings);
        }
        else
        {
            // No existing file => set bad stats in case of a bug
            // (to avoid exploits)
            // Sorry if your save got ruined by this
            Debug.Log("No existing file for " + data.saveFile);
            data.Complete = false;
            data.bestScore = 0;
            data.bestTime = 10000000;
            data.Rank = 0;
            data.CollectedAllRedrings = false;
            for (int i = 0; i < data.RedRings.Length; i += 1)
            {
                data.RedRings[i] = false;
            }
        }
    }
}
