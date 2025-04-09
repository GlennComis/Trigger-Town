using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class NewsController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform textRect;
    public RectTransform viewportRect;
    public TextMeshProUGUI newsLabel;

    [Header("Scrolling Settings")]
    public float scrollSpeed = 100f;

    [Header("News Headlines")]
    [TextArea(2, 5)]
    public List<string> headlines = new List<string>();

    private int currentIndex = -1;
    private float startX;
    private float endX;
    private Tween scrollTween;

    private void Start()
    {
        if (headlines.Count == 0)
        {
            Debug.LogWarning("No headlines assigned to NewsController.");
            return;
        }

        ShowNextHeadline();
    }
    
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.N))
        {
            scrollTween?.Kill();
            ShowNextHeadline();
        }
#endif
    }
    

    private void ShowNextHeadline()
    {
        currentIndex = (currentIndex + 1) % headlines.Count;
        newsLabel.text = headlines[currentIndex];

        LayoutRebuilder.ForceRebuildLayoutImmediate(textRect);

        float textWidth = textRect.rect.width;
        float viewportWidth = viewportRect.rect.width;

        // Start just off the right edge of the viewport
        float startX = viewportWidth;
        float endX = -textWidth;

        textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);

        float totalDistance = startX - endX;
        float duration = totalDistance / scrollSpeed;

        scrollTween?.Kill();

        scrollTween = textRect.DOAnchorPosX(endX, duration)
            .SetEase(Ease.Linear)
            .OnComplete(ShowNextHeadline);
    }

    private void OnDisable()
    {
        scrollTween?.Kill();
    }
}