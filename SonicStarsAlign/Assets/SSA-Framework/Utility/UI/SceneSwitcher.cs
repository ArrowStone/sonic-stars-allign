using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    //private int cachedScene = 0;

    public static SceneSwitcher Instance;

    private PlayerCharacterParameters cachedCharacter;
    private string cachedStageDisplayName;

    public int gameplaySceneIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // this keeps it alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*public void SwitchScene(int _scn)
    {
        if (_scn < 0)
        {
            ExitGame();
        }
        else
        {
            SceneManager.LoadSceneAsync(_scn);
        }
    }
    public void AddScene(int _scn)
    {
        SceneManager.LoadSceneAsync(_scn, LoadSceneMode.Additive);
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

    public void CacheScene(int _scn)
    {
        cachedScene = _scn;
    }
    public void LoadCachedScene()
    {
        SwitchScene(cachedScene);
    }
    public void AddCachedScene()
    {
        AddScene(cachedScene);
    }
    public int GetCachedScene()
    {
        return cachedScene;
    }
    */
    public void CacheCharacter(PlayerCharacterParameters character)
    {
        cachedCharacter = character;
    }

    public PlayerCharacterParameters GetCachedCharacter()
    {
        return cachedCharacter;
    }
    public void CacheStage(string displayName)
    {
        cachedStageDisplayName = displayName;
    }

    public string GetCachedStage()
    {
        return cachedStageDisplayName;
    }
    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
    }
}