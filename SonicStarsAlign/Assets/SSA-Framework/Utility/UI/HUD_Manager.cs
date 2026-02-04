using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

// Simplifies and unifies interaction with the HUD.
public class HUD_Manager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] textObjects;
    [SerializeField]
    private string[] textObjectNames;

    public Sonic_PlayerStateMachine _ctx;

    private void Awake()
    {
        void SetRings(float rings)
        {
            SetText(0, ((int)rings).ToString("D3"));
        }
        void SetScore(float score)
        {
            SetText(1, ((int)score).ToString("D6"));
        }

        _ctx.Chs.RingSet += SetRings;
        _ctx.Chs.ScoreSet += SetScore;
    }

    private void SetText(int i, string text)
    {
        textObjects[i].text = text;
    }
}
