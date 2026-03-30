using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Simplifies and unifies interaction with the HUD.
public class HUD_Manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] textObjects;
    [SerializeField] private GameObject scoreTextPopup;
    [SerializeField] private GameObject scoreImagePopup;
    [SerializeField] private RectTransform Canvas;
    [SerializeField] private RectTransform homingIndicator;
    private Image homingIndicatorImage;
    private Vector3 targetHomingPos;

    public Sprite[] scoreEncouragementImages;
    public int maxPopupCount;
    public float ScorePopupIgnoreBelow;
    public float PopupOffset;
    public Sonic_PlayerStateMachine _ctx;

    private List<GameObject> popups = new List<GameObject>();

    // Homing indicator
    private void FixedUpdate()
    {
        _ctx.HomingCheck();
        if(_ctx.HomingTargetDetector.TargetDetected && 
        Vector3.Angle((_ctx.HomingTargetDetector.TargetOutput.transform.position - _ctx.transform.position).normalized, Camera.main.transform.forward) < 90f)
        {
            homingIndicatorImage.enabled = true;
            targetHomingPos = _ctx.HomingTargetDetector.TargetOutput.transform.position;
        }
        else homingIndicatorImage.enabled = false;
    }

    private void Update()
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(targetHomingPos);
        if(screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height) screenPos = new Vector2(-1000, -1000);

        homingIndicator.position = screenPos;
    }

    // Initialising automatic counters like rings and score
    private void Awake()
    {
        homingIndicatorImage = homingIndicator.GetComponent<Image>();

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
                ScorePopup(6);
            }
            else if(score - prevScore >= 500)
            {
                ScorePopup(4);
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
    }

    private void SetText(int i, string text)
    {
        textObjects[i].text = text;
    }

    // Showing score gain
    public void ScorePopup(string popupText)
    {
        int emptyIndex = popups.FindIndex(popup => popup == null);

        if(emptyIndex == -1 && popups.Count >= maxPopupCount)
        {
            return;
        }

        GameObject newPopup = Instantiate(scoreTextPopup, Canvas);

        if(emptyIndex == -1)
        {
            emptyIndex = popups.Count;
            popups.Add(newPopup);
        }
        else
        {
            popups[emptyIndex] = newPopup;
        }

        newPopup.GetComponentInChildren<TextMeshProUGUI>().text = popupText;
        RectTransform popupTransform = (RectTransform) newPopup.transform;

        Vector2 popupPosition = popupTransform.anchoredPosition;
        popupPosition.y -= PopupOffset * emptyIndex;
        popupTransform.anchoredPosition = popupPosition;
    }

    // Showing encouragement
    public void ScorePopup(int encouragementId)
    {
        int emptyIndex = popups.FindIndex(popup => popup == null);

        Debug.Log(emptyIndex);

        if(emptyIndex == -1 && popups.Count > maxPopupCount)
        {
            return;
        }

        GameObject newPopup = Instantiate(scoreImagePopup, Canvas);

        if(emptyIndex == -1)
        {
            emptyIndex = popups.Count;
            popups.Add(newPopup);
        }
        else
        {
            popups[emptyIndex] = newPopup;
        }

        newPopup.GetComponentInChildren<Image>().sprite = scoreEncouragementImages[encouragementId];
        RectTransform popupTransform = (RectTransform) newPopup.transform;

        Vector2 popupPosition = popupTransform.anchoredPosition;
        popupPosition.y -= PopupOffset * emptyIndex;
        popupTransform.anchoredPosition = popupPosition;
    }
}
