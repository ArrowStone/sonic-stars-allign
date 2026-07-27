using System.Collections;
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

        IEnumerator WaitForAnim()
        {
            // Tried to make it wait for the actual animation to finish but couldn't find a decent way.
            yield return new WaitForSeconds(0.5f);

            SceneSwitcher.Instance.AddScene(loadingScreenSceneIndex);
        }


        StartCoroutine(WaitForAnim());
    }
}
