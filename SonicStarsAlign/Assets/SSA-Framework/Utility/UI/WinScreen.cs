using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

// Handles the presentation of data on the results screen
public class WinScreen : MonoBehaviour
{
    static public WinScreen Instance {get; private set;}

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
    public GoalRing goalRing;

    [SerializeField] private Sprite[] rankLetters;
    [SerializeField] private int[] rankScores;
    [SerializeField] private int targetTime;

    private void Awake()
    {
        if(Instance == null)
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
        PlayerCharacterStats chs = Sonic_PlayerStateMachine.Instance.Chs;
        int score = chs.Score;
        int rings = (int) chs.Rings;
        float time = HUDManager.Instance.stageTimer;
        int ringScore = rings * 100;
        int totalScore = score + Math.Max(0, targetTime - (int) time) * 5;
        int rank = goalRing.RankCalc(Sonic_PlayerStateMachine.Instance);
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

    public void ToggleObjects()
    {
        foreach(GameObject obj in ObjectsToToggle)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
}