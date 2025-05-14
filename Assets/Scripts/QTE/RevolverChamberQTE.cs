using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RevolverChamberQTE : MonoBehaviour, IQTE
{
    #region Events

    public event Action<QTEResult> OnQTEComplete;
    public event Action OnQTEStarted;

    #endregion

    #region UI References

    [Header("Sound Effects")]
    [SerializeField] private AudioClip inputSuccessSFX;
    [SerializeField] private AudioClip inputFailSFX;
    [SerializeField] private AudioSource audioSource;
    
    [Header("UI References")]
    [SerializeField] private GameObject qteParent;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private CanvasGroup interactiveVisuals;

    [Header("QTE Elements")]
    [SerializeField] private RectTransform chamberDisplayParent;
    [SerializeField] private ChamberUI chamberPrefab;
    [SerializeField] private Transform[] chamberAnchors;

    [Header("Slider UI")]
    [SerializeField] private Slider qteTimerSlider;

    #endregion

    #region Config

    [Header("QTE Settings")]
    [Tooltip("Keys that can be randomly selected for the sequence.")]
    [SerializeField] private List<KeyCode> availableKeys;

    [SerializeField] private float inputTimePerChamber = 1.2f;

    [Header("Intro Animation")]
    [SerializeField] private float barSlideDuration = 0.6f;
    [SerializeField] private float backgroundFadeDuration = 0.4f;
    [SerializeField] private float interactiveFadeDuration = 0.3f;
    [SerializeField] private float backgroundTargetAlpha = 154f / 255f;

    [Header("Roll Animation")]
    [SerializeField] private float rollDelay = 0.3f;
    [SerializeField] private float chamberSpinDegrees = 60f;

    #endregion

    #region State

    private int bulletIndex;
    private int currentChamberIndex = 0;
    private List<KeyCode> inputSequence = new();
    private List<ChamberUI> chamberUIs = new();
    private float timer;
    private bool isActive = false;
    private bool canPress = false;
    private List<KeyCode> seededInputs = null;

    #endregion

    #region Unity

    private void Update()
    {
        if (!isActive || !canPress) return;

        timer -= Time.deltaTime;
        if (qteTimerSlider != null)
            qteTimerSlider.value = timer;

        if (timer <= 0f)
        {
            FailQTE();
            return;
        }

        if (Input.GetKeyDown(inputSequence[currentChamberIndex]))
        {
            StartCoroutine(ShortDelayPressLock());
            PlaySFX(inputSuccessSFX);
            chamberUIs[currentChamberIndex].PlayFlash();
            chamberUIs[currentChamberIndex].MarkCorrectAndFade();
            StartCoroutine(RollToNextChamber());
        }
        else if (Input.anyKeyDown)
        {
            FailQTE();
        }
    }
    
    private IEnumerator ShortDelayPressLock()
    {
        canPress = false;
        yield return new WaitForSeconds(0.05f); // Short lockout to prevent input spam
        canPress = true;
    }

    #endregion

    #region IQTE Implementation

    public void InitializeVisualState()
    {
        qteParent.SetActive(false);
        backgroundImage.color = new Color(0, 0, 0, 0);
        topBar.anchoredPosition = new Vector2(-Screen.width, topBar.anchoredPosition.y);
        bottomBar.anchoredPosition = new Vector2(Screen.width, bottomBar.anchoredPosition.y);
        interactiveVisuals.alpha = 0f;

        foreach (Transform child in chamberDisplayParent)
            Destroy(child.gameObject);

        chamberDisplayParent.localRotation = Quaternion.identity;
        inputSequence.Clear();
        chamberUIs.Clear();
        currentChamberIndex = 0;
    }

    public void PlayQTEIntro()
    {
        HUDManager.Instance?.SetDrawText(false);
        StartCoroutine(QTEIntroRoutine());
    }

    public void StartQTE()
    {
        OnQTEStarted?.Invoke();
        gameObject.SetActive(true);
        isActive = true;

        currentChamberIndex = 0;

        GenerateSequence();

        timer = inputTimePerChamber * inputSequence.Count;

        if (qteTimerSlider != null)
        {
            qteTimerSlider.maxValue = timer;
            qteTimerSlider.value = timer;
            qteTimerSlider.gameObject.SetActive(true);
        }

        HighlightChamber(0);
        canPress = true;
    }

    public void PauseQTE() => isActive = false;
    public void ResumeQTE() => isActive = true;

    #endregion

    #region Intro & Setup

    private IEnumerator QTEIntroRoutine()
    {
        qteParent.SetActive(true);
        backgroundImage.DOFade(backgroundTargetAlpha, backgroundFadeDuration);
        yield return new WaitForSeconds(backgroundFadeDuration);

        topBar.DOAnchorPosX(0f, barSlideDuration).SetEase(Ease.OutCubic);
        bottomBar.DOAnchorPosX(0f, barSlideDuration).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(barSlideDuration);

        interactiveVisuals.DOFade(1f, interactiveFadeDuration);
        yield return new WaitForSeconds(interactiveFadeDuration);

        StartQTE();
    }

    public void SetSeededInputs(List<KeyCode> customInputs)
    {
        seededInputs = new List<KeyCode>(customInputs);
    }

    private void GenerateSequence()
    {
        inputSequence.Clear();
        chamberUIs.Clear();

        int sequenceLength = seededInputs != null
            ? seededInputs.Count
            : Random.Range(1, Mathf.Min(chamberAnchors.Length, 6) + 1); // 1 to 6 steps

        bulletIndex = sequenceLength - 1;

        for (int i = 0; i < sequenceLength; i++)
        {
            KeyCode input;

            if (seededInputs != null)
            {
                input = seededInputs[i];
            }
            else
            {
                if (availableKeys == null || availableKeys.Count == 0)
                {
                    Debug.LogError("No availableKeys defined for Revolver QTE!");
                    input = KeyCode.Space;
                }
                else
                {
                    input = availableKeys[Random.Range(0, availableKeys.Count)];
                }
            }

            inputSequence.Add(input);

            // Instantiate chamber at anchor point
            ChamberUI ui = Instantiate(chamberPrefab, chamberDisplayParent);
            if (i < chamberAnchors.Length)
            {
                RectTransform rt = ui.GetComponent<RectTransform>();
                Vector3 localPos = chamberDisplayParent.InverseTransformPoint(chamberAnchors[i].position);
                rt.anchoredPosition = localPos;
            }

            ui.SetKey(input, i == bulletIndex);
            chamberUIs.Add(ui);
        }
    }

    private void HighlightChamber(int index)
    {
        for (int i = 0; i < chamberUIs.Count; i++)
            chamberUIs[i].SetHighlight(i == index);
    }

    #endregion

    #region Rolling Logic

    private IEnumerator RollToNextChamber()
    {
        PlayRollSound();

        currentChamberIndex++;

        if (currentChamberIndex > bulletIndex)
        {
            CompleteQTE(QTEResult.Perfect);
            yield break;
        }

        float targetRotation = -currentChamberIndex * chamberSpinDegrees;
        chamberDisplayParent.DOLocalRotate(new Vector3(0, 0, targetRotation), rollDelay)
            .SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(rollDelay);

        HighlightChamber(currentChamberIndex);
        canPress = true;
    }


    private void PlayRollSound()
    {
        // TODO: Hook up revolver cylinder click sound
    }

    #endregion

    #region Outcome

    private void CompleteQTE(QTEResult result)
    {
        isActive = false;
        if (qteTimerSlider != null)
            qteTimerSlider.gameObject.SetActive(false);

        OnQTEComplete?.Invoke(result);
        InitializeVisualState();
    }

    private void FailQTE()
    {
        PlaySFX(inputFailSFX);
        CompleteQTE(QTEResult.Miss);
    }
    
    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    #endregion
}
