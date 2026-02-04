using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

// Simplifies and unifies interaction with the HUD.
public class HUD_Manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] textObjects;
    [SerializeField] private GameObject scorePopup;
    [SerializeField] private RectTransform Canvas;

    public float scorePopupIgnoreBelow;

    public Sonic_PlayerStateMachine _ctx;

    // Initialising automatic counters like rings and score
    private void Awake()
    {
        void SetRings(float rings, float _)
        {
            SetText(0, $"<mspace=450>{(int)rings:D3}</mspace>");
        }
        void SetScore(float score, float prevScore)
        {
            SetText(1, $"<mspace=370>{(int)score:D6}</mspace>");

            if(score - prevScore < scorePopupIgnoreBelow) return;

            Instantiate(scorePopup, Canvas).GetComponentInChildren<TextMeshProUGUI>().text = (score - prevScore).ToString();
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
