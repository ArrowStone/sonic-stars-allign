using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

// Simplifies and unifies interaction with the HUD.
public class HUD_Manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] textObjects;
    [SerializeField] private GameObject scorePopup;
    [SerializeField] private RectTransform Canvas;
    public float ScorePopupIgnoreBelow;
    public float PopupOffset;
    public Sonic_PlayerStateMachine _ctx;

    private List<GameObject> popups = new List<GameObject>();

    // Initialising automatic counters like rings and score
    private void Awake()
    {
        void SetRings(float rings, float _)
        {
            SetText(0, $"<mspace=500>{(int)rings:D3}</mspace>");
        }
        void SetScore(float score, float prevScore)
        {
            SetText(1, $"<mspace=370>{(int)score:D6}</mspace>");

            if (score - prevScore < ScorePopupIgnoreBelow) return;

            int emptyIndex = popups.FindIndex(popup => popup == null);

            // checking if we already have 3 popups
            if(emptyIndex == -1 && popups.Count > 2)
            {
                return;
            }

            GameObject newPopup = Instantiate(scorePopup, Canvas);

            Debug.Log(emptyIndex);

            if(emptyIndex == -1)
            {
                emptyIndex = popups.Count;
                popups.Add(newPopup);
            }
            else
            {
                popups[emptyIndex] = newPopup;
            }

            newPopup.GetComponentInChildren<TextMeshProUGUI>().text = (score - prevScore).ToString();
            RectTransform popupTransform = (RectTransform) newPopup.transform;

            Vector2 popupPosition = popupTransform.anchoredPosition;
            popupPosition.y -= PopupOffset * emptyIndex;
            popupTransform.anchoredPosition = popupPosition;
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
}
