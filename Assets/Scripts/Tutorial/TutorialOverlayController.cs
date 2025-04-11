using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialOverlayController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private Camera worldCamera;

    [Header("Animation Durations")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float openDuration = 0.4f;
    [SerializeField] private float closeDuration = 0.3f;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float quickMoveDuration = 0.2f;

    [Header("Cutout Settings")]
    [SerializeField] private float cutoutPadding = 0.05f; // Additional viewport padding around the target
    [SerializeField] private float targetOverlayAlpha = 0.7f; // ✅ Controls the max darkness

    private Material overlayMaterial;
    private Tween currentTween;
    private Vector4 currentCenter;
    private float currentRadius;
    
    private const string CutoutCenterProp = "_CutoutCenter";
    private const string CutoutRadiusProp = "_CutoutRadius";
    private const string OverlayAlphaProp = "_OverlayAlpha";

    private void Awake()
    {
        overlayMaterial = Instantiate(overlayImage.material);
        overlayImage.material = overlayMaterial;
        overlayImage.enabled = false;
        
        overlayMaterial.SetVector(CutoutCenterProp, new Vector4(0.5f, 0.5f, 0, 0));
        overlayMaterial.SetFloat(CutoutRadiusProp, 0f);
        overlayMaterial.SetFloat(OverlayAlphaProp, 0f); // Optional reset to invisible on start
    }

    /// <summary>
    /// Call this to start highlighting the first target.
    /// </summary>
    public void ShowFirstHighlight(SpriteRenderer target)
    {
        overlayImage.enabled = true;
        currentTween?.Kill();

        GetTargetData(target, out Vector4 targetCenter, out float targetRadius);
        
        overlayMaterial.SetFloat(CutoutRadiusProp, 0f);
        overlayMaterial.SetVector(CutoutCenterProp, targetCenter);
        overlayMaterial.SetFloat(OverlayAlphaProp, 0f);

        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(DOTween.To(
            () => 0f,
            a => overlayMaterial.SetFloat(OverlayAlphaProp, a),
            targetOverlayAlpha,
            fadeDuration
        ));
        
        sequence.Append(DOTween.To(
            () => 0f,
            r => overlayMaterial.SetFloat(CutoutRadiusProp, r),
            targetRadius,
            openDuration
        ).SetEase(Ease.OutCubic));

        currentCenter = targetCenter;
        currentRadius = targetRadius;
        currentTween = sequence;
    }

    /// <summary>
    /// Call this to move the highlight to the next target.
    /// </summary>
    public void HighlightNext(SpriteRenderer target)
    {
        currentTween?.Kill();

        GetTargetData(target, out Vector4 targetCenter, out float targetRadius);

        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(DOTween.To(
            () => overlayMaterial.GetFloat(CutoutRadiusProp),
            r => overlayMaterial.SetFloat(CutoutRadiusProp, r),
            0f,
            closeDuration
        ).SetEase(Ease.InCubic));
        
        sequence.Append(DOTween.To(
            () => overlayMaterial.GetVector(CutoutCenterProp),
            v => overlayMaterial.SetVector(CutoutCenterProp, v),
            targetCenter,
            quickMoveDuration
        ).SetEase(Ease.OutCubic));
        
        sequence.Append(DOTween.To(
            () => 0f,
            r => overlayMaterial.SetFloat(CutoutRadiusProp, r),
            targetRadius,
            openDuration
        ).SetEase(Ease.OutCubic));

        currentCenter = targetCenter;
        currentRadius = targetRadius;
        currentTween = sequence;
    }

    /// <summary>
    /// Call this when the highlight sequence is complete.
    /// </summary>
    public void HideOverlay()
    {
        currentTween?.Kill();
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(DOTween.To(
            () => overlayMaterial.GetFloat(CutoutRadiusProp),
            r => overlayMaterial.SetFloat(CutoutRadiusProp, r),
            0f,
            closeDuration
        ).SetEase(Ease.InCubic));
        
        sequence.Append(DOTween.To(
            () => overlayMaterial.GetFloat(OverlayAlphaProp),
            a => overlayMaterial.SetFloat(OverlayAlphaProp, a),
            0f,
            fadeDuration
        ).SetEase(Ease.OutQuad));

        sequence.OnComplete(() =>
        {
            overlayImage.enabled = false;
        });

        currentTween = sequence;
    }

    /// <summary>
    /// Converts a SpriteRenderer to a center and radius in viewport space.
    /// </summary>
    private void GetTargetData(SpriteRenderer target, out Vector4 center, out float radius)
    {
        Bounds bounds = target.bounds;
        Vector3 worldCenter = bounds.center;
        Vector3 worldCorner = bounds.center + bounds.extents;

        Vector2 viewportCenter = worldCamera.WorldToViewportPoint(worldCenter);
        Vector2 viewportCorner = worldCamera.WorldToViewportPoint(worldCorner);

        center = new Vector4(viewportCenter.x, viewportCenter.y, 0, 0);
        radius = Vector2.Distance(viewportCenter, viewportCorner) + cutoutPadding;
    }
}
