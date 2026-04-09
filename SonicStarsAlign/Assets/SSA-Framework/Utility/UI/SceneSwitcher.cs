using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    private int cachedScene = 0;

    public static SceneSwitcher Instance;

    private PlayerCharacterParameters cachedCharacter;
    private PlayerCharacterStats cachedStats;
    private string cachedStageDisplayName;

    public int gameplaySceneIndex;

    private void Awake()
    {
        Instance = this;
    }

    public void SwitchScene(int _scn)
    {
        // When in the pause menu
        Time.timeScale = 1f;

        if (_scn < 0)
        {
            ExitGame();
        }
        else
        {
            SceneManager.LoadScene(_scn);
        }
    }
    public void AddScene(int _scn)
    {
        if(_scn < 0)
        {
            ExitGame();
        }
        else 
        {
            SceneManager.LoadSceneAsync(_scn, LoadSceneMode.Additive);
        }
    }
    public void DelScene(int _scn)
    {
        SceneManager.UnloadSceneAsync(_scn);
    }
    private void ExitGame()
    {
        // Credit: Unity Docs
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void AddCachedScene()
    {
        AddScene(cachedScene);
    }

    // Sorry lajeeth had to bring this back so main mneu buttons work

    public void CacheScene(int _scn)
    {
        cachedScene = _scn;
    }
    public void LoadCachedScene()
    {
        SwitchScene(cachedScene);
    }
    public void CacheCharacter(PlayerCharacterParameters character)
    {
        cachedCharacter = character;
    }

    public PlayerCharacterParameters GetCachedCharacter()
    {
        return cachedCharacter;
    }
    public void CacheStats(PlayerCharacterStats stats)
    {
        cachedStats = stats;
    }

    public PlayerCharacterStats GetCachedStats()
    {
        return cachedStats;
    }
    public void CacheStage(string displayName)
    {
        cachedStageDisplayName = displayName;
    }

    public string GetCachedStage()
    {
        return cachedStageDisplayName;
    }
}