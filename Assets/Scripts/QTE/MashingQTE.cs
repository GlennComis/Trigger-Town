using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MashingQTE : MonoBehaviour, IQTE
{
    [Header("Config")]
    [SerializeField] private int requiredPresses = 15;
    [SerializeField] private float qteDuration = 5f;

    [Header("UI")]
    [SerializeField] private GameObject qteParent;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private CanvasGroup visuals;

    public event System.Action<QTEResult> OnQTEComplete;
    public event System.Action OnQTEStarted;

    private float timer;
    private int pressCount;
    private bool isActive;

    private void Awake()
    {
        PlayQTEIntro();
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        timerSlider.value = timer;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pressCount++;
            progressSlider.value = pressCount;

            if (pressCount >= requiredPresses)
            {
                Finish(QTEResult.Perfect);
            }
        }

        if (timer <= 0f)
        {
            EvaluateResult();
        }
    }

    public void InitializeVisualState()
    {
        qteParent.SetActive(false);
        visuals.alpha = 0f;
        visuals.interactable = false;
        visuals.blocksRaycasts = false;
    }

    public void PlayQTEIntro()
    {
        StartCoroutine(QTEIntroRoutine());
    }

    public void StartQTE()
    {
        pressCount = 0;
        timer = qteDuration;
        isActive = true;

        timerSlider.maxValue = qteDuration;
        timerSlider.value = qteDuration;

        progressSlider.maxValue = requiredPresses;
        progressSlider.value = 0;

        OnQTEStarted?.Invoke();
    }

    public void PauseQTE() => isActive = false;
    public void ResumeQTE() => isActive = true;

    private IEnumerator QTEIntroRoutine()
    {
        qteParent.SetActive(true);
        visuals.alpha = 0f;
        visuals.DOFade(1f, 0.3f);

        yield return new WaitForSeconds(0.4f);
        StartQTE();
    }

    private void EvaluateResult()
    {
        isActive = false;

        QTEResult result;
        float percent = (float)pressCount / requiredPresses;

        if (percent >= 1f) result = QTEResult.Perfect;
        else if (percent >= 0.6f) result = QTEResult.Good;
        else result = QTEResult.Miss;

        Finish(result);
    }

    private void Finish(QTEResult result)
    {
        isActive = false;
        visuals.DOFade(0f, 0.3f).OnComplete(() =>
        {
            qteParent.SetActive(false);
            OnQTEComplete?.Invoke(result);
        });
    }
}
