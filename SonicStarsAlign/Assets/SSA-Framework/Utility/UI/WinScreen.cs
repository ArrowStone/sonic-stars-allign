using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

// Handles the presentation of data on the results screen
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

    [SerializeField] private Sprite[] rankLetters;
    [SerializeField] private int targetTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
        }
        RankImage.sprite = rankLetters[rank];

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