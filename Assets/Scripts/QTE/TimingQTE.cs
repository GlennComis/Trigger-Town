using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimingQTE : MonoBehaviour, IQTE
{
    #region Events

    public event System.Action<QTEResult> OnQTEComplete;
    public event System.Action OnQTEStarted;

    #endregion

    #region UI References

    [Header("UI References")]
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform sweetSpot;
    [SerializeField] private RectTransform sliderBar;
    [SerializeField] private Slider qteTimerSlider;

    [Header("UI Elements")]
    [SerializeField] private GameObject qteParent;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private CanvasGroup interactiveVisuals;

    #endregion

    #region Configuration

    [Header("Slider Settings")]
    [SerializeField] private float moveSpeed = 400f;
    [SerializeField] private float perfectThreshold = 5f;

    [Header("Sweet Spot")]
    [SerializeField] private float sweetSpotWidth = 40f;

    [Header("Marker Constraints")]
    [SerializeField] private float leftOffset = 20f;
    [SerializeField] private float rightOffset = 20f;

    [Header("Timing")]
    [SerializeField] private float qteDuration = 10f;

    [Header("Bar Animation Settings")]
    [SerializeField] private float barSlideDuration = 0.6f;
    [SerializeField] private float backgroundFadeDuration = 0.4f;
    [SerializeField] private float interactiveFadeDuration = 0.3f;
    [SerializeField] private float backgroundTargetAlpha = 154f / 255f;

    #endregion

    #region Private State

    private float timer = 0f;
    private float minX;
    private float maxX;
    private bool movingRight = true;
    private bool isActive = false;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeVisualState();
    }

    private void Update()
    {
        if (!isActive) return;

        MoveMarker();
        UpdateTimer();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EvaluateResult();
        }
    }

    #endregion

    #region IQTE Implementation

    public void InitializeVisualState()
    {
        qteParent.SetActive(false);

        var bgColor = backgroundImage.color;
        bgColor.a = 0f;
        backgroundImage.color = bgColor;

        topBar.anchoredPosition = new Vector2(-Screen.width, topBar.anchoredPosition.y);
        bottomBar.anchoredPosition = new Vector2(Screen.width, bottomBar.anchoredPosition.y);
        interactiveVisuals.alpha = 0f;
    }

    public void PlayQTEIntro()
    {
        HUDManager.Instance.SetDrawText(false);
        StartCoroutine(QTEIntroRoutine());
    }

    public void StartQTE()
    {
        isActive = true;
        OnQTEStarted?.Invoke();
        gameObject.SetActive(true);

        timer = qteDuration;
        if (qteTimerSlider != null)
        {
            qteTimerSlider.maxValue = qteDuration;
            qteTimerSlider.value = qteDuration;
            qteTimerSlider.gameObject.SetActive(true);
        }

        InitMarker();
        InitSweetSpot();
    }

    public void PauseQTE()
    {
        isActive = false;
    }

    public void ResumeQTE()
    {
        isActive = true;
    }

    #endregion

    #region QTE Setup & Animation

    private IEnumerator QTEIntroRoutine()
    {
        qteParent.SetActive(true);

        backgroundImage.color = new Color(0, 0, 0, 0);
        backgroundImage.DOFade(backgroundTargetAlpha, backgroundFadeDuration);
        yield return new WaitForSeconds(backgroundFadeDuration);

        topBar.DOAnchorPosX(0f, barSlideDuration).SetEase(Ease.OutCubic);
        bottomBar.DOAnchorPosX(0f, barSlideDuration).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(barSlideDuration);

        interactiveVisuals.DOFade(1f, interactiveFadeDuration);
        yield return new WaitForSeconds(interactiveFadeDuration);

        StartQTE();
    }

    private void InitMarker()
    {
        float barWidth = sliderBar.rect.width;
        float halfBar = barWidth / 2f;

        minX = -halfBar + leftOffset;
        maxX = halfBar - rightOffset;

        marker.anchoredPosition = new Vector2(minX, marker.anchoredPosition.y);
        movingRight = true;
    }

    private void InitSweetSpot()
    {
        sweetSpot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sweetSpotWidth);
        float sweetHalf = sweetSpotWidth / 2f;

        float sweetMinLimit = minX + sweetHalf;
        float sweetMaxLimit = maxX - sweetHalf;

        float randomX = Random.Range(sweetMinLimit, sweetMaxLimit);
        sweetSpot.anchoredPosition = new Vector2(randomX, sweetSpot.anchoredPosition.y);
    }

    #endregion

    #region QTE Evaluation

    private void MoveMarker()
    {
        float direction = movingRight ? 1f : -1f;
        marker.anchoredPosition += new Vector2(direction * moveSpeed * Time.deltaTime, 0f);

        if (marker.anchoredPosition.x >= maxX) movingRight = false;
        else if (marker.anchoredPosition.x <= minX) movingRight = true;
    }

    private void UpdateTimer()
    {
        timer -= Time.deltaTime;
        if (qteTimerSlider != null)
            qteTimerSlider.value = timer;

        if (timer <= 0f)
        {
            TimeoutMiss();
        }
    }

    private void EvaluateResult()
    {
        isActive = false;
        InitializeVisualState();
        if (qteTimerSlider != null) qteTimerSlider.gameObject.SetActive(false);

        float markerX = marker.anchoredPosition.x;
        float sweetX = sweetSpot.anchoredPosition.x;
        float sweetHalf = sweetSpotWidth / 2f;

        float min = sweetX - sweetHalf;
        float max = sweetX + sweetHalf;

        QTEResult result = QTEResult.Miss;
        if (markerX >= min && markerX <= max)
        {
            float dist = Mathf.Abs(markerX - sweetX);
            result = dist <= perfectThreshold ? QTEResult.Perfect : QTEResult.Good;
        }

        OnQTEComplete?.Invoke(result);
    }

    private void TimeoutMiss()
    {
        isActive = false;
        InitializeVisualState();
        if (qteTimerSlider != null) qteTimerSlider.gameObject.SetActive(false);

        OnQTEComplete?.Invoke(QTEResult.Miss);
    }

    #endregion
}
