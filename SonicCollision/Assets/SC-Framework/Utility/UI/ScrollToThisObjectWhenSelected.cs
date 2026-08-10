using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Scrolls the ScrollView in settings to the selected object because Unity has no builtin way to do this
public class ScrollToThisObjectWhenSelected : MonoBehaviour, ISelectHandler
{
    private ScrollRect SettingsScrollView;
    private Rect Viewport;
    private RectTransform Content;
    private RectTransform parent;
    void Awake()
    {
        if (gameObject.scene.buildIndex != 3)
        {
            // Don't bother if not in settings
            Destroy(this);
            return;
        }

        parent = transform.parent as RectTransform;

        Transform parentIteration = transform;
        // Element - Text - Tab - Content - Viewport
        for (int i = 0; i < 3; i += 1)
        {
            parentIteration = parentIteration.parent;
        }

        Content = parentIteration.transform as RectTransform;
        Viewport = (Content.parent as RectTransform).rect;
        SettingsScrollView = Content.parent.parent.GetComponent<ScrollRect>();

        if (SettingsScrollView.name != "Scroll View") Debug.LogWarning(string.Format("Focus scroll script: {0} at Scroll View position", parentIteration.name), gameObject);

    }

    // Where the scrolling happens
    public void OnSelect(BaseEventData data)
    {
        float belowBy = parent.anchoredPosition.y + Content.anchoredPosition.y + Viewport.height;
        float aboveBy = parent.anchoredPosition.y + Content.anchoredPosition.y;
        float outsideBy = Math.Min(belowBy, 0f) + Math.Max(aboveBy, 0f);

        SettingsScrollView.verticalNormalizedPosition += outsideBy * 3f / Content.rect.height;
        SettingsScrollView.verticalNormalizedPosition = Mathf.Clamp(SettingsScrollView.verticalNormalizedPosition, 0f, 1f);
    }
}
