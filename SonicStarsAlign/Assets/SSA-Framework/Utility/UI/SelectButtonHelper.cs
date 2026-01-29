using UnityEngine;

public class SelectButtonHelper : MonoBehaviour
{
    public StageSelector selector;           // assign StageSelector
    public int loadingScreenSceneIndex;      // only the loading screen

    public void LoadSelected()
    {
        if (selector == null || StageSelector.Instance == null) return;

        string stageName = selector.GetSelectedStage().displayName;
        Debug.Log("Caching Stage: " + stageName);

        SceneSwitcher.Instance.CacheStage(stageName);
        SceneSwitcher.Instance.SwitchScene(loadingScreenSceneIndex);
    }
}
