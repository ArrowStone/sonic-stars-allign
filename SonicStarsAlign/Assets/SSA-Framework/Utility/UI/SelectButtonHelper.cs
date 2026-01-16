using UnityEngine;

public class SelectButtonHelper : MonoBehaviour
{
    public StageSelector selector; // assign your StageSelector object
    public SceneSwitcher switcher; // assign your SceneSwitcher object

    public void LoadSelected()
    {
        int sceneIndex = selector.GetSelectedSceneIndex();
        switcher.SwitchScene(sceneIndex);
    }
}
