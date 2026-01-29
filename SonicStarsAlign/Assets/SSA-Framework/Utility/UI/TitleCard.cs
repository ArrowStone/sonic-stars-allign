using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleCard : MonoBehaviour
{
    //public Image stagePreview;
    public Image stageNameImage;
    public float delaySeconds = 3f;

    void Start()
    {
        StartCoroutine(LoadStageCoroutine());
    }

    IEnumerator LoadStageCoroutine()
    {
        string cachedStage = SceneSwitcher.Instance?.GetCachedStage();

        if (string.IsNullOrEmpty(cachedStage))
        {
            Debug.LogError("No cached stage found!");
            yield break;
        }

        if (StageSelector.Instance == null)
        {
            Debug.LogError("StageSelector singleton missing!");
            yield break;
        }

        var stage = StageSelector.Instance.GetStageByDisplayName(cachedStage);
        if (stage == null)
        {
            Debug.LogError("Stage not found for displayName: " + cachedStage);
            yield break;
        }

        // Update UI
        //if (stagePreview != null && stage.preview != null) stagePreview.sprite = stage.preview;
        if (stageNameImage != null && stage.loadingName != null) stageNameImage.sprite = stage.loadingName;

        // Wait for a few seconds
        yield return new WaitForSeconds(delaySeconds);

        // Load the stage scene
        Debug.Log("Loading scene: " + stage.sceneIndex + " (" + stage.displayName + ")");
        AsyncOperation op = SceneManager.LoadSceneAsync(stage.sceneIndex, LoadSceneMode.Single);
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;
    }
}
