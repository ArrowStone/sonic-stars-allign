using UnityEngine;

// Obtains the required data to be saved as well as setting the saved values when loading.
[CreateAssetMenu]
public class GameStateSaver : ScriptableObject
{
    public StageData[] StageDataAssets;
    public void LoadData()
    {
        if(StageDataAssets == null) return;
        
        for(int i=0; i<StageDataAssets.Length; i++)
        {
            DataSaver.ReadStageData(StageDataAssets[i]);
        }
    }

    void OnEnable()
    {
        LoadData();
    }
}
