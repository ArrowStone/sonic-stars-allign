using UnityEngine;
using UnityEngine.UI;

public class RedRingUnlockButton : MonoBehaviour
{
    public Button Button;
    public VideoScreenManager VideoScreenManager;

    private void Start()
    {
        Button.interactable = SceneSwitcher.Instance.GetRedRingUnlock();
    }

    public void OnButtonPressed()
    {
        if (SceneSwitcher.Instance.GetRedRingUnlock())
            VideoScreenManager.PlayVideo();
    }
}