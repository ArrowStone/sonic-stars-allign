using UnityEngine;
using TMPro;
public class WinScreen : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI ringsText;
    public TextMeshProUGUI rankText;

    private string[] rankLetters = { "D", "C", "B", "A", "S" };

    private void Start()
    {
        var data = SceneSwitcher.Instance.GetWinData();
        scoreText.text = "Score: " + data.score;
        timeText.text = "Time: " + FormatTime(data.time);
        ringsText.text = "Rings: " + (int)data.rings;
        rankText.text = "Rank: " + rankLetters[data.rank];
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time - 0.5f) % 60;
        int decimals = (int)(time * 100 % 100);
        if (decimals > 99) decimals -= 100;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, decimals);
    }
}