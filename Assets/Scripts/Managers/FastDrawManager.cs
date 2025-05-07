using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

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
    [SerializeField] private float postCountdownMaxDelay = 2f;
    [SerializeField] private float countdownStepDelay = 1.2f;
    private WaitForSeconds timeBetweenRounds = new WaitForSeconds(3f);

    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Camera Tension Settings")]
    [Tooltip("Maximum sway distance (camera side-to-side and up-down shake).")]
    [SerializeField] private float swayAmplitude = 0.05f;

    [Tooltip("How quickly the camera sways back and forth.")]
    [SerializeField] private float swayFrequency = 1f;

    [Tooltip("How much the camera's FOV zooms in and out during tension.")]
    [SerializeField] private float microZoomAmplitude = 1f;

    [Tooltip("Speed at which the FOV pulses (zoom in/out frequency).")]
    [SerializeField] private float microZoomSpeed = 2f;

    [Tooltip("How long it takes to ramp up the intensity of camera effects.")]
    [SerializeField] private float intensityRampTime = 3f;

    [Tooltip("Additional zoom during countdown to build tension.")]
    [SerializeField] private float countdownZoomAmount = 2f;

    [Header("Controllers")]
    [SerializeField] private EnemyController currentEnemyController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RewardSystemController rewardSystemController;
    public RewardSystemController RewardSystemController => rewardSystemController;

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
    private Coroutine cameraEffectRoutine;

    private bool canShoot = false;
    private bool hasResult = false;
    private bool fightEnded = false;
    private bool drawStarted = false;

    private int playerStreak = 0;
    private int longestPlayerStreak = 0;
    private bool flawlessGame = true;
    private float fastestDrawTime = -1f;

    private float originalFOV;
    private Vector3 originalCameraPosition;

    private IQTE activeQTE;
    private TutorialControllerFastDraw tutorialControllerFastDraw;

    #endregion

    #region Public Fields

    public bool isActionPaused = false;
    public GameObject healthIndicatorPrefab;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        tutorialControllerFastDraw = GetComponent<TutorialControllerFastDraw>();

        if (cinemachineCamera != null)
        {
            originalFOV = cinemachineCamera.Lens.FieldOfView;
            originalCameraPosition = cinemachineCamera.transform.position;
        }
    }

    private void Start()
    {
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

        if (cinemachineCamera != null)
            StartCoroutine(ResetCameraSmoothly(0.5f));
    }

    private IEnumerator DrawSequence()
    {
        hasResult = false;
        canShoot = false;
        drawStarted = true;

        float countdownDuration = countdownStepDelay * 3f;
        float delay = Random.Range(0f, postCountdownMaxDelay);

        if (cinemachineCamera != null)
        {
            if (cameraEffectRoutine != null)
                StopCoroutine(cameraEffectRoutine);

            originalCameraPosition = cinemachineCamera.transform.position;
            cameraEffectRoutine = StartCoroutine(CameraMicroEffects(intensityRampTime));
            StartCoroutine(ZoomInDuringCountdown(countdownDuration));
        }

        for (int count = 3; count >= 2; count--)
        {
            HUDManager.Instance.ShowCountdownNumber(count);
            PlayCountdownBeep();
            yield return new WaitForSeconds(countdownStepDelay);
        }

        HUDManager.Instance.ShowCountdownNumber(1);
        PlayCountdownBeep();

        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(delay);
        ResetTimeScale();

        PlayDrawSound();
        HUDManager.Instance.SetDrawText(true);
        HUDManager.Instance.HideCountdown();

        if (cameraEffectRoutine != null)
        {
            StopCoroutine(cameraEffectRoutine);
            cameraEffectRoutine = null;
            StartCoroutine(ResetCameraSmoothly(0.75f));
        }

        drawStartTime = Time.time;
        drawStarted = false;
        canShoot = true;

        activeDrawSequenceRoutine = null;
        OnDrawSignal?.Invoke();
    }

    private IEnumerator CameraMicroEffects(float rampDuration)
    {
        float elapsedTime = 0f;
        float rampTime = 0f;

        Vector3 initialPosition = originalCameraPosition;
        float initialFOV = originalFOV;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (true)
        {
            float delta = Time.unscaledDeltaTime;
            elapsedTime += delta;
            rampTime = Mathf.Min(rampTime + delta, rampDuration);
            float intensity = Mathf.Clamp01(rampTime / rampDuration);

            float swayX = Mathf.Sin(elapsedTime * swayFrequency) * swayAmplitude * intensity;
            float swayY = Mathf.Cos(elapsedTime * swayFrequency * 0.8f) * swayAmplitude * intensity;
            float zoomOffset = Mathf.Sin(elapsedTime * microZoomSpeed) * microZoomAmplitude * intensity;

            cinemachineCamera.transform.localPosition = initialPosition + new Vector3(swayX, swayY, 0f);
            cinemachineCamera.Lens.FieldOfView = initialFOV + zoomOffset;

            yield return wait;
        }
    }


    private IEnumerator ZoomInDuringCountdown(float duration)
    {
        if (cinemachineCamera == null) yield break;

        float startFOV = originalFOV;
        float targetFOV = originalFOV - countdownZoomAmount;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, easedT);
            yield return null;
        }
    }

    private IEnumerator ResetCameraSmoothly(float duration = 0.75f)
    {
        if (cinemachineCamera == null) yield break;

        Vector3 startPosition = cinemachineCamera.transform.position;
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            cinemachineCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, easedT);
            cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, originalFOV, easedT);

            yield return null;
        }

        cinemachineCamera.transform.position = originalCameraPosition;
        cinemachineCamera.Lens.FieldOfView = originalFOV;
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    #endregion

    #region QTE Integration

    public void PlayerShot()
    {
        if (isActionPaused) return;

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

    public void SubscribeResultHandle()
    {
        if (activeQTE != null)
        {
            activeQTE.OnQTEComplete -= HandleQTEResult;
            activeQTE.OnQTEComplete += HandleQTEResult;
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
        
        if(!tutorialControllerFastDraw.isInTutorial)
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
            
            if(!tutorialControllerFastDraw.isInTutorial)
                HUDManager.Instance.SetWinScreen(rewardSystemController.GetRewards());
        }
        else
        {
            HUDManager.Instance.SetDefeatScreen();
        }
    }

    #endregion

    #region Utility

    private void StopActiveRoutine()
    {
        if (activeDrawSequenceRoutine != null)
        {
            StopCoroutine(activeDrawSequenceRoutine);
            activeDrawSequenceRoutine = null;
        }

        if (cameraEffectRoutine != null)
        {
            StopCoroutine(cameraEffectRoutine);
            cameraEffectRoutine = null;
        }

        if (cinemachineCamera != null)
        {
            StartCoroutine(ResetCameraSmoothly(0.5f));
        }

        ResetTimeScale();
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

    public void PauseAction()
    {
        Debug.Log("Pause?");
        isActionPaused = true;
        HUDManager.Instance.HideCountdown();
    }

    public void ResumeAction() => isActionPaused = false;

    public void PlayerShootAnimation() => playerController.Shoot();
    public void SetEnemyReactionTime(float reactionTime) => enemyReactionTime = reactionTime;
    public void SetQTEType(QTEType qteType) => currentQTEType = qteType;
    public bool IsPassiveEnemy() => currentEnemyController.IsPassiveEnemy();

    #endregion
}
