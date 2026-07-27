using UnityEngine;
using System.Collections;
public class UIAnimations : MonoBehaviour
{
    public Vector2 startOffset = new Vector2(-800f, 0f); // off-screen left
    public float duration = 0.6f;
    public float delay = 0f;
    public bool fadeIn = false;

    RectTransform rect;
    CanvasGroup canvasGroup;
    Vector2 endPos;
    Coroutine routine;

    void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        Canvas.ForceUpdateCanvases(); // ensure layout is applied

        endPos = rect.anchoredPosition;

        // Move off-screen before first frame
        rect.anchoredPosition = endPos + startOffset;

        // Start invisible if fading
        if (fadeIn && canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(AnimateIn());
    }

    IEnumerator AnimateIn()
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Slide
            rect.anchoredPosition = Vector2.Lerp(endPos + startOffset, endPos, smoothT);

            // Fade
            if (fadeIn && canvasGroup != null)
                canvasGroup.alpha = smoothT;

            yield return null;
        }

        // Ensure final state
        rect.anchoredPosition = endPos;
        if (fadeIn && canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}
