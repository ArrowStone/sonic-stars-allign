using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Simplifies and unifies interaction with the HUD.
public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] textObjects;
    [SerializeField] private GameObject scoreTextPopup;
    [SerializeField] private GameObject scoreImagePopup;
    [SerializeField] private RectTransform Canvas;
    [SerializeField] private RectTransform HomingReticle;
    [SerializeField] private RectTransform SecondHomingReticle;
    [SerializeField] private TMP_Text TimeCounter;
    private bool usingFirstReticle;
    private GameObject previousFrameTarget;
    private Animator reticleAnimator;
    private Animator secondReticleAnimator;
    private Vector3 targetHomingPos;
    private Vector3 previousHomingTarget;
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
        usingFirstReticle = true;

        popups = new GameObject[maxPopupCount];

        reticleAnimator = HomingReticle.GetComponent<Animator>();
        secondReticleAnimator = SecondHomingReticle.GetComponent<Animator>();

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

        stageTimer += _delta;

        int minutes = (int) (stageTimer / 60);
        int seconds = (int) (stageTimer - 0.5f) % 60;
        int decimals = (int) (stageTimer * 100 % 100);
        if(decimals > 99) decimals -= 100;

        TimeCounter.text = string.Format("<mspace=330>{0:00}:{1:00}.{2:00}</mspace>", 
            minutes,
            seconds, 
            decimals);
    }

    private void LateUpdate()
    {
        GameObject currentTarget = _ctx.HomingTargetDetector.TargetOutput;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(_ctx.homingTargetPosition);
        if( _ctx.HomingTargetDetector.TargetDetected &&
            !(_ctx.CurrentEstate == PlayerStates.RailGrinding && _ctx.homingOntoSpline))
        {
            // Target detected
            Animator anim = usingFirstReticle ? reticleAnimator : secondReticleAnimator;
            if(currentTarget != previousFrameTarget)
            {
                // Different target -> switch reticles & disable the old one
                usingFirstReticle = !usingFirstReticle;
                previousFrameTarget = currentTarget;
                previousHomingTarget = targetHomingPos;
                anim.SetBool("Active", false);
                anim = usingFirstReticle ? reticleAnimator : secondReticleAnimator;
            }
            
            anim.SetBool("Active", true);
            targetHomingPos = _ctx.homingTargetPosition;
            //previousFrameTarget = currentTarget;
        }
        else
        {
            // No target
            reticleAnimator.SetBool("Active", false);
            secondReticleAnimator.SetBool("Active", false);
        }

        RectTransform reticle = usingFirstReticle ? HomingReticle : SecondHomingReticle;
        // Checking if the fading reticle's on-screen
        if(viewportPos.x is > -1 and < 1 && viewportPos.y is > -1 and < 1 && viewportPos.z > 2f)
        {
            // On-screen
            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetHomingPos);
            reticle.position = screenPos;
        }
        else
        {
            // Off-screen -> instantly disable
            Animator anim = usingFirstReticle ? reticleAnimator : secondReticleAnimator;
            anim.SetTrigger("InstantDisable");
            anim.SetBool("Active", false);
        }

        RectTransform secReticle = usingFirstReticle ? SecondHomingReticle : HomingReticle;
        Vector3 secViewportPos = Camera.main.WorldToViewportPoint(previousHomingTarget);
        // Checking if the fading reticle's on-screen
        if(secViewportPos.x is > -1 and < 1 && secViewportPos.y is > -1 and < 1 && secViewportPos.z > 2f)
        {
            // On-screen
            Vector2 secScreenPos = Camera.main.WorldToScreenPoint(previousHomingTarget);
            secReticle.position = secScreenPos;
        }
        else
        {
            // Off-screen -> instantly disable
            Animator secAnim = usingFirstReticle ? secondReticleAnimator : reticleAnimator;
            secAnim.SetTrigger("InstantDisable");
        }
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
