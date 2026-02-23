using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public PlayerCharacterParameters characterParameters;
    public int gameplaySceneIndex;

    public void OnCharacterSelected()
    {
        SceneSwitcher.Instance.CacheCharacter(characterParameters);
        SceneSwitcher.Instance.SwitchScene(gameplaySceneIndex);
    }
}