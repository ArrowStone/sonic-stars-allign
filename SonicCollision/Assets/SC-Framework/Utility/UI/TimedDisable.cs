using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Used in the pause menu's settings button so you couldn't spam
public class TimedDisable : MonoBehaviour
{
    [SerializeField] Button button;
    public void Timer(float time = 5f)
    {
        IEnumerator DaThing() // I'm tired
        {
            button.enabled = false;
            yield return new WaitForSecondsRealtime(time);
            button.enabled = true;
        }
        StartCoroutine(DaThing());
    }
}
