using UnityEngine;
using DG.Tweening;
using System;

public class BeatNote : MonoBehaviour
{
    public RectTransform rect;
    public bool hasTriggeredLate;
    public bool isJudged;

    private Tweener tween;
    private float travelTime;
    private bool isActive;
    private Action<BeatNote> onComplete;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Activate(Vector2 spawnPos, float travelTime, float despawnX, Action<BeatNote> onComplete)
    {
        this.travelTime = travelTime;
        this.onComplete = onComplete;

        rect.anchoredPosition = spawnPos;
        gameObject.SetActive(true);

        isActive = true;
        hasTriggeredLate = false;
        isJudged = false;

        tween?.Kill();

        tween = rect.DOAnchorPosX(despawnX, travelTime)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isActive = false;
                onComplete?.Invoke(this);
            });
    }

    public void Deactivate()
    {
        tween?.Kill();
        gameObject.SetActive(false);
        isActive = false;
    }

    public float GetTimingOffset()
    {
        if (!isActive || tween == null || !tween.IsActive())
            return float.MaxValue;

        float progress = tween.ElapsedPercentage();
        float offset = Mathf.Abs(progress - 0.5f);  //When it is 50% in the tween we are half way (so exactly in the middle)
        return offset * travelTime;
    }

    public bool IsInTimeWindow(float allowedSeconds)
    {
        return GetTimingOffset() <= allowedSeconds;
    }

    public bool HasPassedCenter()
    {
        if (!isActive || tween == null || !tween.IsActive()) 
            return false;

        return tween.ElapsedPercentage() > 0.5f;
    }
}