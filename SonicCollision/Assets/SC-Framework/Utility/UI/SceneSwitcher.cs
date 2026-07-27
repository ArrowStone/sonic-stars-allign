using UnityEngine;
using UnityEngine.SceneManagement;

// A script which is just supposed to tackle switching scenes using events and animations.
// However, it kind of grew out of control.
public class SceneSwitcher : MonoBehaviour
{
    private int cachedScene = 0;

    public static SceneSwitcher Instance;

    private PlayerCharacterParameters cachedCharacter;
    private PlayerCharacterStats cachedStats;
    private string cachedStageDisplayName;

    public int gameplaySceneIndex;

    private int cachedScore;
    private float cachedTime;
    private float cachedTargetTime;
    private float cachedRings;
    private int cachedRank;
    private bool[] cachedRedRings;
    private bool redRingsUnlocked = false;

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
        if (_scn < 0)
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

    // Sorry lajeeth had to bring this back so main menu would buttons work

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
    public void CacheWinData(int score, float time, float targetTime, float rings, int rank, bool[] redRings)
    {
        Debug.Log(string.Format("Score: {0}, Time: {1}; Rings: {2}, Rank: {3}", score, time, rings, rank));
        cachedScore = score;
        cachedTime = time;
        cachedTargetTime = targetTime;
        cachedRings = rings;
        cachedRank = rank;
        cachedRedRings = redRings;
    }
    public (int score, float time, float targetTime, float rings, int rank, bool[] redRings) GetWinData()
    {
        return (cachedScore, cachedTime, cachedTargetTime, cachedRings, cachedRank, cachedRedRings);
    }
    public void SetRedRingUnlock()
    {
        redRingsUnlocked = true;
    }

    public bool GetRedRingUnlock()
    {
        return redRingsUnlocked;
    }
}