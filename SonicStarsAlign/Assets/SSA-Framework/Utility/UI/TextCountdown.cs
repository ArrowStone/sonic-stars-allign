using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextCountdown : MonoBehaviour
{
    private TMP_Text textComponent;
    public UnityEvent StepEvent;
    public UnityEvent DoneEvent;
    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    public void Countdown(float time, int startValue, int targetValue = 0, float stepTime = 0.1f)
    {
        StartCoroutine(CountdownCoroutine(time, startValue, targetValue, stepTime));
    }

    public IEnumerator CountdownCoroutine(float time, int startValue, int targetValue = 0, float stepTime = 0.1f, string format = "{0}")
    {
        float startTime = Time.time;
        float currentValue = startValue;
        float step = (targetValue - startValue) / time * stepTime;

        while (Time.time - startTime < time)
        {
            textComponent.text = string.Format(format, (int)currentValue);
            currentValue += step;
            yield return new WaitForSeconds(stepTime);
            if (step != 0) StepEvent.Invoke();
        }
        textComponent.text = string.Format(format, (int)targetValue);
        DoneEvent.Invoke();
    }

    public IEnumerator TimeCountdownCoroutine(float time, float startValue, float targetValue = 0f, float stepTime = 0.1f, string format = "{0:00}:{1:00}.{2:00}")
    {
        float startTime = Time.time;
        float currentValue = startValue;

        while (Time.time - startTime < time)
        {
            textComponent.text = FormatTime(currentValue, format);
            currentValue += (targetValue - startValue) / time * stepTime;
            yield return new WaitForSeconds(stepTime);
            StepEvent.Invoke();
        }
        textComponent.text = FormatTime(targetValue, format);
        DoneEvent.Invoke();
    }

    public void SetValue(int value, string format = "{0}")
    {
        textComponent.text = string.Format(format, value);
    }

    public void SetTime(float time, string format = "{0:00}:{1:00}.{2:00}")
    {
        textComponent.text = FormatTime(time, format);
    }

    private string FormatTime(float time, string format = "{0:00}:{1:00}.{2:00}")
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time - 0.5f) % 60;
        int decimals = (int)(time * 100 % 100);
        if (decimals > 99) decimals -= 100;
        return string.Format(format, minutes, seconds, decimals);
    }
}
