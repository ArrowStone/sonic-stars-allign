using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.UI;

// Simplifies and unifies interaction with the HUD.
public class HUD_Manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] textObjects;
    [SerializeField] private GameObject scoreTextPopup;
    [SerializeField] private GameObject scoreImagePopup;
    [SerializeField] private RectTransform Canvas;
    [SerializeField] private RectTransform HomingReticle;
    [SerializeField] private TMP_Text TimeCounter;
    private Animator homingReticleAnimator;
    private Vector3 targetHomingPos;

    public Sprite[] scoreEncouragementImages;
    public int maxPopupCount;
    public float ScorePopupIgnoreBelow;
    public float PopupOffset;
    public float stageTimer;
    public Sonic_PlayerStateMachine _ctx;

    private GameObject[] popups;

    // Initialising automatic counters like rings and score
    private void Awake()
    {
        popups = new GameObject[maxPopupCount];

        homingReticleAnimator = HomingReticle.GetComponent<Animator>();

        void SetRings(float rings, float _)
        {
            SetText(0, $"<mspace=500>{(int)rings:D3}</mspace>");
        }
        void SetScore(float score, float prevScore)
        {
            SetText(1, $"<mspace=370>{(int)score:D6}</mspace>");

            if (score - prevScore < ScorePopupIgnoreBelow) return;

            ScorePopup((score - prevScore).ToString());

            if(score - prevScore >= 700)
            {
                ScorePopup(2);
            }
            else if(score - prevScore >= 500)
            {
                ScorePopup(1);
            }
            else if(score - prevScore >= 200)
            {
                ScorePopup(0);
            }
            
        }

        _ctx.Chs.RingSet += SetRings;
        _ctx.Chs.ScoreSet += SetScore;

        _ctx.Chs.Rings = _ctx.Chs.Rings;
        _ctx.Chs.Score = _ctx.Chs.Score;

        stageTimer = 0;
    }

    // Homing indicator
    private void FixedUpdate()
    {
        float _delta = Time.fixedDeltaTime;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(_ctx.homingTargetPosition);
        if( _ctx.HomingTargetDetector.TargetDetected &&
            viewportPos.x is > -1 and < 1 && viewportPos.y is > -1 and < 1 && viewportPos.z > 0 &&
            !(_ctx.CurrentEstate == PlayerStates.RailGrinding && _ctx.homingOntoSpline))
        {
            homingReticleAnimator.SetBool("Active", true);
            targetHomingPos = _ctx.homingTargetPosition;
        }
        else homingReticleAnimator.SetBool("Active", false);

        stageTimer += _delta;
    }

    private void Update()
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(targetHomingPos);
        if(screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height) 
            screenPos = new Vector2(-1000, -1000);

        HomingReticle.position = screenPos;

        int minutes = (int) (stageTimer / 60);
        int seconds = (int) (stageTimer - 0.5f) % 60;
        int decimals = (int) (stageTimer * 100 % 100);
        if(decimals > 99) decimals -= 100;

        TimeCounter.text = string.Format("<mspace=330>{0:00}:{1:00}.{2:00}</mspace>", 
            minutes,
            seconds, 
            decimals);
    }

    private void SetText(int i, string text)
    {
        textObjects[i].text = text;
    }

    // Showing score gain
    public void ScorePopup(string popupText)
    {
        Debug.Log(popupText);
        int emptyIndex = Array.FindIndex(popups, i => i == null);
        Debug.Log(emptyIndex);

        foreach (GameObject gb in popups)
        {
            Debug.Log(gb);
        }

        if(emptyIndex == -1)
        {
            Debug.Log("No space!");
            return;
        }

        GameObject newPopup = Instantiate(scoreTextPopup, Canvas);

        popups[emptyIndex] = newPopup;

        newPopup.GetComponentInChildren<TextMeshProUGUI>().text = popupText;
        RectTransform popupTransform = (RectTransform) newPopup.transform;

        Vector2 popupPosition = popupTransform.anchoredPosition;
        popupPosition.y -= PopupOffset * emptyIndex;
        popupTransform.anchoredPosition = popupPosition;
    }

    // Showing encouragement
    public void ScorePopup(int encouragementId)
    {
        int emptyIndex = Array.FindIndex(popups, i => i == null);

        Debug.Log(emptyIndex);

        if(emptyIndex == -1)
        {
            Debug.Log("No room for a popup!");
            return;
        }

        GameObject newPopup = Instantiate(scoreImagePopup, Canvas);

        popups[emptyIndex] = newPopup;

        newPopup.GetComponentInChildren<Image>().sprite = scoreEncouragementImages[encouragementId];
        RectTransform popupTransform = (RectTransform) newPopup.transform;

        Vector2 popupPosition = popupTransform.anchoredPosition;
        popupPosition.y -= PopupOffset * emptyIndex;
        popupTransform.anchoredPosition = popupPosition;
    }
}
