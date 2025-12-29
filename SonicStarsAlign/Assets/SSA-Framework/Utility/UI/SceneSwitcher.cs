using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    private int cachedScene = 0;

    public void SwitchScene(int _scn)
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
}