using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadeTransitionController : MonoBehaviour
{
    [SerializeField] private RawImage transitionImage;
    [SerializeField] private RectTransform curtainPanel;
    [Range(0.1f, 5f)] public float fadeDuration = 1.5f;

    private Tween currentTween;

    public void FadeIn(FadeType type, Action onComplete = null)
    {
        currentTween?.Kill();

        switch (type)
        {
            case FadeType.Simple:
                transitionImage.gameObject.SetActive(true);
                transitionImage.color = new Color(0, 0, 0, 1);
                currentTween = transitionImage.DOFade(0, fadeDuration).OnComplete(() =>
                {
                    DOVirtual.DelayedCall(0.01f, () =>
                    {
                        transitionImage.gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
                });
                break;

            case FadeType.Curtain:
                curtainPanel.localScale = Vector3.one;
                curtainPanel.gameObject.SetActive(true);
                currentTween = curtainPanel.DOScaleY(0, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    curtainPanel.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
                break;
        }
    }

    public void FadeOut(FadeType type, Action onComplete = null)
    {
        currentTween?.Kill();

        switch (type)
        {
            case FadeType.Simple:
                transitionImage.gameObject.SetActive(true);
                transitionImage.color = new Color(0, 0, 0, 0);
                currentTween = transitionImage.DOFade(1, fadeDuration).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
                break;

            case FadeType.Curtain:
                curtainPanel.localScale = new Vector3(1, 0, 1);
                curtainPanel.gameObject.SetActive(true);
                currentTween = curtainPanel.DOScaleY(1, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
                break;
        }
    }

    private void OnValidate()
    {
        if (transitionImage)
        {
            transitionImage.color = new Color(0, 0, 0, 0);
        }

        if (curtainPanel)
        {
            curtainPanel.localScale = new Vector3(1, 0, 1);
            curtainPanel.pivot = new Vector2(0.5f, 1f);
        }
    }

    [ContextMenu("Fade In - Simple")]
    private void TestFadeInSimple() => FadeIn(FadeType.Simple);

    [ContextMenu("Fade Out - Simple")]
    private void TestFadeOutSimple() => FadeOut(FadeType.Simple);

    [ContextMenu("Fade In - Curtain")]
    private void TestFadeInCurtain() => FadeIn(FadeType.Curtain);

    [ContextMenu("Fade Out - Curtain")]
    private void TestFadeOutCurtain() => FadeOut(FadeType.Curtain);
}

public enum FadeType
{
    Simple,
    Curtain
}
