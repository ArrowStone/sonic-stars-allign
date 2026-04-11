using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelector : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        public StageData data;     // saved data like best score and time
        public string displayName; // optional for debug/logging
        public int sceneIndex;     // scene build index
        public Sprite preview;     // thumbnail image
        public Sprite nameSprite;  // stage name image
        public Sprite loadingName; // loading name image
    }

    [Header("Stages Setup")]
    public Stage[] stages;

    [Header("UI References")]
    public Image stagePreview;     // UI Image for stage thumbnail
    public Image stageNameImage;   // UI Image for stage name graphic
    public Image[] redRingIcons;
    public TMP_Text bestTimeText;
    public TMP_Text bestScoreText;

    private int currentIndex = 0;

    public static StageSelector Instance;

    private void Awake()
    {
        /*if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }*/
        
        if (Instance != null) {Destroy(Instance);}
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (stages.Length == 0)
        {
            Debug.LogWarning("No stages set up in StageSelector!");
            return;
        }
        UpdateStageUI();
    }

    // Cycle to the next stage
    public void NextStage()
    {
        if (stages.Length == 0) return;

        currentIndex = (currentIndex + 1) % stages.Length;
        UpdateStageUI();
    }

    // Cycle to the previous stage
    public void PreviousStage()
    {
        if (stages.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = stages.Length - 1;

        UpdateStageUI();
    }

    // Updates the UI with the currently selected stage
    void UpdateStageUI()
    {
        Stage current = stages[currentIndex];

        // Update stage preview
        if (stagePreview != null && current.preview != null)
            stagePreview.sprite = current.preview;

        // Update stage name image
        if (stageNameImage != null && current.nameSprite != null)
            stageNameImage.sprite = current.nameSprite;

        for(int i=0; i<redRingIcons.Length; i++)
        {
            Debug.Log(current.data.RedRings[i]);
            redRingIcons[i].color = current.data.RedRings[i] ? Color.red : Color.white;
        }

        float bestTime = current.data.bestTime;
        int minutes = (int) (bestTime / 60);
        int seconds = (int) (bestTime - 0.5f) % 60;
        int decimals = (int) (bestTime * 100 % 100);
        if(decimals > 99) decimals -= 100;

        bestTimeText.text = string.Format("{0:00}:{1:00}.{2:00}", 
            minutes,
            seconds, 
            decimals);
        bestScoreText.text = current.data.bestScore.ToString();

        // Optional: debug log
        Debug.Log("Selected Stage: " + current.displayName + " (SceneIndex: " + current.sceneIndex + ")");
    }
    public Stage GetSelectedStage()
    {
        if (stages.Length == 0) return null;
        return stages[currentIndex];
    }
    public Stage GetStageByDisplayName(string displayName)
    {
        foreach (var stage in stages)
        {
            if (stage.displayName == displayName)
                return stage;
        }
        return null;
    }
}
