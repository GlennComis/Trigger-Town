using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimingQTE : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform sweetSpot;
    [SerializeField] private RectTransform sliderBar;
    [SerializeField] private Slider qteTimerSlider; // ✅ Timer slider

    [Header("Slider Settings")]
    [SerializeField] private float moveSpeed = 400f;
    [SerializeField] private float perfectThreshold = 5f;

    [Header("Sweet Spot")]
    [SerializeField] private float sweetSpotWidth = 40f;

    [Header("Marker Constraints")]
    [SerializeField] private float leftOffset = 20f;
    [SerializeField] private float rightOffset = 20f;

    [Header("Timing")]
    [SerializeField] private float qteDuration = 10f; // ✅ Total allowed time

    private float timer = 0f;
    private float minX;
    private float maxX;
    private bool movingRight = true;
    private bool isActive = false;

    public System.Action<QTEResult> OnQTEComplete;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject qteParent;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private CanvasGroup interactiveVisuals;

    [Header("Bar Animation Settings")]
    [SerializeField] private float barSlideDuration = 0.6f;
    [SerializeField] private float backgroundFadeDuration = 0.4f;
    [SerializeField] private float interactiveFadeDuration = 0.3f;
    [SerializeField] private float backgroundTargetAlpha = 154f / 255f;
    
    private void Start()
    {
        PrepareIntroState();
    }
    

    public void PlayQTEIntro()
    {
        UIManager.Instance.SetDrawText(false);
        StartCoroutine(QTEIntroRoutine());
    }

    public void PrepareIntroState()
    {
        qteParent.SetActive(false);
        Color bgColor = backgroundImage.color;
        bgColor.a = 0f;
        backgroundImage.color = bgColor;
        topBar.anchoredPosition = new Vector2(-Screen.width, topBar.anchoredPosition.y);
        bottomBar.anchoredPosition = new Vector2(Screen.width, bottomBar.anchoredPosition.y);
        interactiveVisuals.alpha = 0f;
    }
    
    private IEnumerator QTEIntroRoutine()
    {
        // Enable UI parent
        qteParent.SetActive(true);

        // Fade background alpha to 154
        backgroundImage.color = new Color(0, 0, 0, 0);
        backgroundImage.DOFade(backgroundTargetAlpha, backgroundFadeDuration);
        yield return new WaitForSeconds(backgroundFadeDuration);

        // Move top bar from offscreen top-left
        Vector2 topStartPos = new Vector2(-Screen.width, topBar.anchoredPosition.y);
        topBar.anchoredPosition = topStartPos;
        topBar.DOAnchorPosX(0f, barSlideDuration).SetEase(Ease.OutCubic);

        // Move bottom bar from offscreen bottom-right
        Vector2 bottomStartPos = new Vector2(Screen.width, bottomBar.anchoredPosition.y);
        bottomBar.anchoredPosition = bottomStartPos;
        bottomBar.DOAnchorPosX(0f, barSlideDuration).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(barSlideDuration);

        // Fade in interactive visuals
        interactiveVisuals.alpha = 0f;
        interactiveVisuals.DOFade(1f, interactiveFadeDuration);
        yield return new WaitForSeconds(interactiveFadeDuration);

        // Start QTE
        StartQTE();
    }

    public void StartQTE()
    {
        isActive = true;
        gameObject.SetActive(true);

        // Init Timer
        timer = qteDuration;
        if (qteTimerSlider != null)
        {
            qteTimerSlider.maxValue = qteDuration;
            qteTimerSlider.value = qteDuration;
            qteTimerSlider.gameObject.SetActive(true);
        }

        float barWidth = sliderBar.rect.width;
        float halfBar = barWidth / 2f;

        minX = -halfBar + leftOffset;
        maxX = halfBar - rightOffset;

        marker.anchoredPosition = new Vector2(minX, marker.anchoredPosition.y);
        movingRight = true;

        // Set sweet spot size and position
        sweetSpot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sweetSpotWidth);
        float sweetHalfWidth = sweetSpotWidth / 2f;

        float sweetMinLimit = minX + sweetHalfWidth;
        float sweetMaxLimit = maxX - sweetHalfWidth;
        float randomX = Random.Range(sweetMinLimit, sweetMaxLimit);
        sweetSpot.anchoredPosition = new Vector2(randomX, sweetSpot.anchoredPosition.y);
    }

    private void Update()
    {
        if (!isActive) return;

        // Marker movement
        float dir = movingRight ? 1f : -1f;
        marker.anchoredPosition += new Vector2(dir * moveSpeed * Time.deltaTime, 0f);

        if (marker.anchoredPosition.x >= maxX) movingRight = false;
        else if (marker.anchoredPosition.x <= minX) movingRight = true;

        // Timer countdown
        timer -= Time.deltaTime;
        if (qteTimerSlider != null)
            qteTimerSlider.value = timer;

        if (timer <= 0f)
        {
            TimeoutMiss();
            return;
        }

        // Input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EvaluateResult();
        }
    }

    private void EvaluateResult()
    {
        isActive = false;
        PrepareIntroState();
        if (qteTimerSlider != null) qteTimerSlider.gameObject.SetActive(false);

        float markerX = marker.anchoredPosition.x;
        float sweetCenterX = sweetSpot.anchoredPosition.x;
        float sweetHalfWidth = sweetSpotWidth / 2f;

        float sweetMin = sweetCenterX - sweetHalfWidth;
        float sweetMax = sweetCenterX + sweetHalfWidth;

        QTEResult result;

        if (markerX >= sweetMin && markerX <= sweetMax)
        {
            float distanceToCenter = Mathf.Abs(markerX - sweetCenterX);
            result = distanceToCenter <= perfectThreshold ? QTEResult.Perfect : QTEResult.Good;
        }
        else
        {
            result = QTEResult.Miss;
        }

        OnQTEComplete?.Invoke(result);
    }

    private void TimeoutMiss()
    {
        isActive = false;
        PrepareIntroState();
        if (qteTimerSlider != null) qteTimerSlider.gameObject.SetActive(false);

        Debug.Log("QTE timed out: auto miss");
        OnQTEComplete?.Invoke(QTEResult.Miss);
    }
}

public enum QTEResult
{
    Miss,
    Good,
    Perfect
}
