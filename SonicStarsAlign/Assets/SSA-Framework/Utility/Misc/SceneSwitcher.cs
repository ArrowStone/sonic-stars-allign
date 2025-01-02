using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSwitcher : MonoBehaviour
{
    public void SwitchScene(int _scn)
    {
        SceneManager.LoadSceneAsync(_scn);
    }
}