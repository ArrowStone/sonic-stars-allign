using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Character Data")]
    public PlayerCharacterParameters[] characterParameters;
    public PlayerCharacterStats[] characterStats;

    [Header("UI")]
    public RectTransform[] characterButtons;
    public Image portraitImage;
    public Sprite[] portraits;

    [Header("Button Sizes")]
    public float normalWidth = 200f;
    public float selectedWidth = 320f;

    public int gameplaySceneIndex;

    int selectedCharacter = -1;

    void Start()
    {
        SelectCharacter(0);
    }

    public void CharacterButtonPressed(int index)
    {
        // If clicking a different character → just select them
        if (selectedCharacter != index)
        {
            SelectCharacter(index);
        }
        else
        {
            // Clicking the same character again → start game
            OnCharacterSelected();
        }
    }

    void SelectCharacter(int index)
    {
        selectedCharacter = index;

        // Resize buttons
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (i == index)
                characterButtons[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, selectedWidth);
            else
                characterButtons[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, normalWidth);
        }

        // Change portrait
        portraitImage.sprite = portraits[index];
    }

    public void OnCharacterSelected()
    {
        SceneSwitcher.Instance.CacheCharacter(characterParameters[selectedCharacter]);
        SceneSwitcher.Instance.CacheStats(characterStats[selectedCharacter]);
        SceneSwitcher.Instance.SwitchScene(gameplaySceneIndex);
    }
}