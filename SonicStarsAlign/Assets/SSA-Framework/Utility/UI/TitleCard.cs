using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TitleCard : MonoBehaviour
{
    //public Image stagePreview;
    public Image stageNameImage;

    public TMP_Text loadingText;
    public float minDisplayTime = 1.5f; // ensures loading screen shows briefly

    void Start()
    {
        StartCoroutine(LoadStageCoroutine());
    }

    IEnumerator LoadStageCoroutine()
    {
        yield return null;
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

        AsyncOperation op = SceneManager.LoadSceneAsync(stage.sceneIndex);
        op.allowSceneActivation = false;

        float elapsedTime = 0f;
        float displayedProgress = 0f;

        while (!op.isDone)
        {
            elapsedTime += Time.deltaTime;

            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                targetProgress,
                Time.deltaTime * 0.5f // speed of increase
            );

            int percent = Mathf.RoundToInt(displayedProgress * 100f);

            if (loadingText != null)
                loadingText.text = $"Loading {percent}%";

            if (displayedProgress >= 1f && elapsedTime >= minDisplayTime)
            {
                loadingText.text = "Loading 100%";
                yield return new WaitForSeconds(0.25f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
