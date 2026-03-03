using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public PlayerCharacterParameters characterParameters;
    public PlayerCharacterStats characterStats;
    public int gameplaySceneIndex;

    public void OnCharacterSelected()
    {
        SceneSwitcher.Instance.CacheCharacter(characterParameters);
        SceneSwitcher.Instance.CacheStats(characterStats);
        SceneSwitcher.Instance.SwitchScene(gameplaySceneIndex);
    }
    public void TestClick()
    {
        Debug.Log("BUTTON CLICKED");
    }
}