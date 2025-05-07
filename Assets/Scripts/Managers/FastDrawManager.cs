using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class FastDrawManager : SingletonMonoBehaviour<FastDrawManager>
{
    #region Events
    public static event System.Action OnDrawSignal;
    public static event System.Action<bool> OnDrawResult;
    public static event System.Action OnFiredEarly;
    public static event System.Action OnQTEStarted;
    public static event System.Action OnQTEReset;
    #endregion

    #region Serialized Fields

    [Header("Draw Countdown")]
    [SerializeField] private int countdownStartNumber = 3;
    [SerializeField] private float countdownStepDelay = 1.2f;
    [SerializeField] private float postCountdownMinDelay = 1f;
    [SerializeField] private float postCountdownMaxDelay = 2f;

    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float countdownZoomAmount = 20f;

    [Header("Camera Tension Settings")]
    [SerializeField] private float swayAmplitude = 0.5f;
    [SerializeField] private float swayFrequency = 1f;

    [Header("Controllers")]
    [SerializeField] private EnemyController currentEnemyController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RewardSystemController rewardSystemController;

    [Header("Quick Time Events")]
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private QTEType currentQTEType = QTEType.Timing;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip countdownBeepClip;
    [SerializeField] private AudioClip drawClip;

    #endregion

    #region Private Fields

    private float drawStartTime;
    private float enemyReactionTime = -1f;
    private Coroutine activeDrawSequenceRoutine;
    private Coroutine swayRoutine;
    private bool canShoot = false;
    private bool hasResult = false;
    private bool fightEnded = false;
    private bool drawStarted = false;
    private int playerStreak = 0;
    private int longestPlayerStreak = 0;
    private bool flawlessGame = true;
    private float fastestDrawTime = -1f;
    private float originalFOV;
    private IQTE activeQTE;
    private TutorialControllerFastDraw tutorialControllerFastDraw;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

    private readonly WaitForSeconds timeBetweenRounds = new WaitForSeconds(3f);

    #endregion

    #region Public Fields
    public bool isActionPaused = false;
    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        tutorialControllerFastDraw = GetComponent<TutorialControllerFastDraw>();

        if (cinemachineCamera != null)
        {
            originalFOV = cinemachineCamera.Lens.FieldOfView;
            cinemachineBasicMultiChannelPerlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Start()
    {
        StartDraw();

        if (tutorialControllerFastDraw.IsInTutorial())
            StartDraw();
    }

    #endregion

    #region Draw Lifecycle

    public void StartDraw()
    {
        if (fightEnded || drawStarted) return;
        drawStarted = true;
        activeDrawSequenceRoutine = StartCoroutine(DrawSequence());
    }

    public void StopDraw()
    {
        StopActiveRoutine();
        drawStarted = false;
        canShoot = false;
        hasResult = false;

        HUDManager.Instance.SetDrawText(false);
        ResetTimeScale();
        SetCameraSway(0f, 0f);
        ResetCameraFOV();
    }

    private IEnumerator DrawSequence()
    {
        hasResult = false;
        canShoot = false;
        drawStarted = true;

        float delay = Random.Range(postCountdownMinDelay, postCountdownMaxDelay);
        SetCameraSway(swayAmplitude, swayFrequency);
        ZoomCameraFOV(originalFOV - countdownZoomAmount, countdownStartNumber * countdownStepDelay);

        for (int count = countdownStartNumber; count >= 1; count--)
        {
            HUDManager.Instance.ShowCountdownNumber(count);
            PlayCountdownBeep();
            yield return new WaitForSeconds(countdownStepDelay);
        }

        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(delay);
        ResetTimeScale();

        PlayDrawSound();
        HUDManager.Instance.SetDrawText(true);
        HUDManager.Instance.HideCountdown();

        SetCameraSway(0f, 0f);
        ResetCameraFOV();

        drawStartTime = Time.time;
        drawStarted = false;
        canShoot = true;

        activeDrawSequenceRoutine = null;
        OnDrawSignal?.Invoke();
    }

    #endregion

    #region QTE Integration

    public void PlayerShot()
    {
        if (isActionPaused || (DialogueManager.instance_exists && DialogueManager.Instance.IsInConversation)) return;

        if (!canShoot)
        {
            PlayerShootAnimation();
            OnFiredEarly?.Invoke();
            StopActiveRoutine();
            return;
        }

        float reactionTime = Time.time - drawStartTime;

        if (reactionTime > enemyReactionTime && !IsPassiveEnemy())
        {
            canShoot = false;
            DetermineFirstShooter(false);
            return;
        }

        canShoot = false;
        PauseAction();

        activeQTE = qteManager.GetQTE(currentQTEType);
        if (activeQTE == null)
        {
            Debug.LogError($"No QTE found for type {currentQTEType}");
            return;
        }

        activeQTE.OnQTEComplete += HandleQTEResult;
        activeQTE.InitializeVisualState();
        OnQTEStarted?.Invoke();
        activeQTE.PlayQTEIntro();
    }

    private void HandleQTEResult(QTEResult result)
    {
        ResumeAction();

        if (activeQTE != null)
            activeQTE.OnQTEComplete -= HandleQTEResult;

        if (result == QTEResult.Good || result == QTEResult.Perfect)
        {
            float reactionTime = Time.time - drawStartTime;
            if (fastestDrawTime < 0f || reactionTime < fastestDrawTime)
                fastestDrawTime = reactionTime;

            PlayerShootAnimation();
            DetermineFirstShooter(true);
        }
        else
        {
            DetermineFirstShooter(false);
        }
    }

    #endregion

    #region Outcome Handling

    public void DetermineFirstShooter(bool isPlayer = false)
    {
        if (hasResult) return;
        hasResult = true;

        if (isPlayer)
        {
            playerStreak++;
            if (playerStreak > longestPlayerStreak)
                longestPlayerStreak = playerStreak;
        }
        else
        {
            flawlessGame = false;
        }

        OnDrawResult?.Invoke(isPlayer);

        if (!tutorialControllerFastDraw.isInTutorial)
            StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        StopActiveRoutine();
        yield return timeBetweenRounds;
        HUDManager.Instance.SetDrawText(false);
        StartDraw();
        OnQTEReset?.Invoke();
    }

    public void RoundEnd(bool playerWon)
    {
        HUDManager.Instance.SetDrawText(false);
        fightEnded = true;
        StopActiveRoutine();

        rewardSystemController.CalculateRewards(currentEnemyController.GetReward(), flawlessGame, fastestDrawTime, longestPlayerStreak);

        if (playerWon)
        {
            PauseAction();
            if (!tutorialControllerFastDraw.isInTutorial)
                HUDManager.Instance.SetWinScreen(rewardSystemController.GetRewards());
        }
        else
        {
            HUDManager.Instance.SetDefeatScreen();
        }
    }

    #endregion

    #region Utility

    public void SubscribeResultHandle()
    {
        if (activeQTE != null)
        {
            activeQTE.OnQTEComplete -= HandleQTEResult;
            activeQTE.OnQTEComplete += HandleQTEResult;
        }
    }

    private void StopActiveRoutine()
    {
        if (activeDrawSequenceRoutine != null)
        {
            StopCoroutine(activeDrawSequenceRoutine);
            activeDrawSequenceRoutine = null;
        }

        if (swayRoutine != null)
        {
            StopCoroutine(swayRoutine);
            swayRoutine = null;
        }

        SetCameraSway(0f, 0f);
        ResetCameraFOV();
        ResetTimeScale();
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void ResetRoundStateForRetry()
    {
        hasResult = false;
        canShoot = true;
        drawStarted = false;
        enemyReactionTime = -1f;
    }

    private void PlayCountdownBeep()
    {
        if (sfxAudioSource != null && countdownBeepClip != null)
            sfxAudioSource.PlayOneShot(countdownBeepClip);
    }

    private void PlayDrawSound()
    {
        if (sfxAudioSource != null && drawClip != null)
            sfxAudioSource.PlayOneShot(drawClip);
    }

    private void SetCameraSway(float amplitude, float frequency)
    {
        if (cinemachineBasicMultiChannelPerlin == null) return;

        if (swayRoutine != null)
        {
            StopCoroutine(swayRoutine);
            swayRoutine = null;
        }

        cinemachineBasicMultiChannelPerlin.FrequencyGain = frequency;
        swayRoutine = StartCoroutine(RampCameraSway(amplitude, countdownStartNumber * countdownStepDelay));
    }

    private IEnumerator RampCameraSway(float targetAmplitude, float duration)
    {
        if (cinemachineBasicMultiChannelPerlin == null) yield break;

        float elapsed = 0f;
        float start = 0f;

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = Mathf.Lerp(start, targetAmplitude, t);
            yield return null;
        }

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = targetAmplitude;
    }

    private void ZoomCameraFOV(float targetFOV, float duration)
    {
        if (cinemachineCamera != null)
        {
            DOTween.To(() => cinemachineCamera.Lens.FieldOfView,
                       fov => cinemachineCamera.Lens.FieldOfView = fov,
                       targetFOV, duration).SetEase(Ease.InOutSine);
        }
    }

    private void ResetCameraFOV()
    {
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Lens.FieldOfView = originalFOV;
        }
    }

    public void PauseAction() => isActionPaused = true;
    public void ResumeAction() => isActionPaused = false;
    public void PlayerShootAnimation() => playerController.Shoot();
    public void SetEnemyReactionTime(float reactionTime) => enemyReactionTime = reactionTime;
    public void SetQTEType(QTEType qteType) => currentQTEType = qteType;
    public bool IsPassiveEnemy() => currentEnemyController.IsPassiveEnemy();

    #endregion
}
