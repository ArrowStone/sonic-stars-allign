using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

// Handles the presentation of data on the results screen and saving stats
public class WinScreen : MonoBehaviour
{
    static public WinScreen Instance { get; private set; }

    [SerializeField] string CounterFormat;
    [SerializeField] string TimeCounterFormat;
    [SerializeField] float CounterStep;
    public TextCountdown scoreCounter;
    public TextCountdown ringCounter;
    public TextCountdown timeCounter;
    public TextCountdown totalScoreCounter;
    public Image RankImage;
    public AudioSource MusicSource;
    public GameObject[] ObjectsToToggle;
    public Image[] RedRingIcons;
    public GameObject CutsceneUnlockPopup;
    private GameStateManager GameStateSaver;
    [SerializeField] private StageData Data;

    [SerializeField] private Sprite[] RankLetters;
    [SerializeField] private int TargetTime;
    [SerializeField] private Sprite RedRingOn;
    [SerializeField] private Sprite RedRingOff;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        GameStateSaver = FindAnyObjectByType<GameStateManager>();
    }

    public void SetInitialValues()
    {
        int score = 0;
        int rings = 0;
        float time = 0;
        scoreCounter.SetValue(score, CounterFormat);
        ringCounter.SetValue(rings, CounterFormat);
        timeCounter.SetTime(time, TimeCounterFormat);
        totalScoreCounter.SetValue(0, CounterFormat);
    }

    public void Activate()
    {
        // Giving the worst stats by default in case there's a bug
        // so people won't exploit it.
        // Sorry to anyone who gets their run screwed up by this.
        int score = 0;
        int rings = 0;
        float time = 10000;
        int ringScore = 0;
        int totalScore = 0;
        int rank = 0;
        bool[] redRings = new bool[0];
        SceneSwitcher ScSw = SceneSwitcher.Instance;

        // ScSw unavailablee when launching the scene on its own through the Editor.
        if (ScSw)
        {
            var data = SceneSwitcher.Instance.GetWinData();
            score = data.score;
            rings = (int)data.rings;
            time = data.time;
            ringScore = rings * 5;
            totalScore = score + Math.Max(0, (int)(data.targetTime - time)) * 5;
            rank = data.rank;
            redRings = data.redRings;
        }
        RankImage.sprite = RankLetters[rank];

        bool collectedAllRings = true;
        if (redRings.Length > 0)
        {
            for (int i = 0; i < RedRingIcons.Length; i += 1)
            {
                RedRingIcons[i].sprite = redRings[i] ? RedRingOn : RedRingOff;
                collectedAllRings = collectedAllRings && redRings[i];
            }
        }
        else collectedAllRings = false;

        CutsceneUnlockPopup.SetActive(collectedAllRings && !Data.CollectedAllRedrings);

        Data.CollectedAllRedrings = collectedAllRings;

        if (!(GameStateSaver && Data))
        {
            // Probably launched the scene in editor, don't bother
            Debug.LogWarning(string.Format("Skipping saving progress due to absence of {0}{1}{2}.",
                                            GameStateSaver ? "" : "GameStateSaver",
                                            !(GameStateSaver || Data) ? " and " : "", // I'm so clever
                                            Data ? "" : "Data"));
        }
        else
        {
            GameStateSaver.UpdateStageData(Data, (byte)rank);
            Data.Complete = true;
            DataSaving.RecordStageData(Data);
        }

        IEnumerator CountdownWait()
        {
            yield return scoreCounter.CountdownCoroutine(1f, 0, score, CounterStep, CounterFormat);
            yield return ringCounter.CountdownCoroutine(1f, 0, rings, CounterStep, CounterFormat);
            yield return timeCounter.TimeCountdownCoroutine(1f, 0f, time, CounterStep, TimeCounterFormat);
        }
        IEnumerator TotalScoreWait()
        {
            yield return totalScoreCounter.CountdownCoroutine(1f, 0, score - ringScore, CounterStep, CounterFormat);
            yield return totalScoreCounter.CountdownCoroutine(1f, score - ringScore, score, CounterStep, CounterFormat);
            yield return totalScoreCounter.CountdownCoroutine(1f, score, totalScore, CounterStep, CounterFormat);
        }

        StartCoroutine(CountdownWait());
        StartCoroutine(TotalScoreWait());
    }

    // Have to do this because animation events do a stupid
    public void PlayMusic()
    {
        MusicSource.Play();
    }

    // Purely for the HUD and other UI elements
    public void ToggleObjects()
    {
        foreach (GameObject obj in ObjectsToToggle)
        {
            obj.SetActive(false);
        }
        // Manually because it's just one
        PauseManager.Instance.enabled = false;
    }
}