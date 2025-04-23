using System.Collections;
using UnityEngine;

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

    [Header("Draw Time")]
    [SerializeField] private float minDrawTime = 5f;
    [SerializeField] private float maxDrawTime = 10f;

    [Header("Controllers")]
    [SerializeField] private EnemyController currentEnemyController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RewardSystemController rewardSystemController;

    [Header("Quick Time Events")]
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private QTEType currentQTEType = QTEType.Timing;

    #endregion

    #region Private Fields

    private float drawStartTime;
    private float enemyReactionTime = -1f;

    private Coroutine activeDrawSequenceRoutine;

    private bool canShoot = false;
    private bool hasResult = false;
    private bool fightEnded = false;
    private bool drawStarted = false;

    private int playerStreak = 0;
    private int longestPlayerStreak = 0;
    private bool flawlessGame = true;
    private float fastestDrawTime = -1f;

    private TutorialControllerFastDraw tutorialControllerFastDraw;
    private IQTE activeQTE;

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

        UIManager.Instance.SetDrawText(false);
    }

    private IEnumerator DrawSequence()
    {
        hasResult = false;
        canShoot = false;

        yield return new WaitForSeconds(Random.Range(minDrawTime, maxDrawTime));

        drawStartTime = Time.time;
        drawStarted = false;
        canShoot = true;

        UIManager.Instance.SetDrawText(true);
        OnDrawSignal?.Invoke();
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
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        StopActiveRoutine();

        yield return new WaitForSeconds(1f);

        UIManager.Instance.SetDrawText(false);
        StartDraw();
        OnQTEReset?.Invoke();
    }

    public void RoundEnd(bool playerWon)
    {
        UIManager.Instance.SetDrawText(false);
        fightEnded = true;

        StopActiveRoutine();
        rewardSystemController.CalculateRewards(flawlessGame, fastestDrawTime, longestPlayerStreak);

        if (playerWon)
        {
            PauseAction();
            UIManager.Instance.SetWinScreen(rewardSystemController.GetRewards());
        }
        else
        {
            UIManager.Instance.SetDefeatScreen();
        }
    }

    #endregion

    #region Utility Methods

    private void StopActiveRoutine()
    {
        if (activeDrawSequenceRoutine != null)
        {
            StopCoroutine(activeDrawSequenceRoutine);
            activeDrawSequenceRoutine = null;
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
