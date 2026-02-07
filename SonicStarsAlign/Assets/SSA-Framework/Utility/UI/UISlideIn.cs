using UnityEngine;
using System.Collections;

public class UISlideIn : MonoBehaviour
{
    public Vector2 startOffset = new Vector2(-800f, 0f); // off-screen left
    public float duration = 0.6f;
    public float delay = 0f;

    RectTransform rect;
    Vector2 endPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        endPos = rect.anchoredPosition;
        rect.anchoredPosition = endPos + startOffset;
    }

    void Start()
    {
        StartCoroutine(SlideIn());
    }

    IEnumerator SlideIn()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);

            // Smooth easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            rect.anchoredPosition = Vector2.Lerp(endPos + startOffset, endPos, smoothT);

            yield return null;
        }

        rect.anchoredPosition = endPos;
    }
}
