using System.Collections;
using UnityEngine;

public class FastDrawManager : SingletonMonoBehaviour<FastDrawManager>
{
    public GameObject healthIndicatorPrefab;
    public static event System.Action OnDrawSignal;
    public static event System.Action<bool> OnDrawResult;
    public static event System.Action OnFiredEarly;

    [Header("Draw Time")]
    [SerializeField] private float minDrawTime = 5f;
    [SerializeField] private float maxDrawTime = 10f;
    private float drawStartTime;
    
    private bool canShoot = false;
    private bool hasResult = false;
    
    private int playerStreak = 0;
    private int longestPlayerStreak = 0;
    private bool flawlessGame = true;
    private float fastestDrawTime = -1f;

    private Coroutine activeDrawSequenceRoutine;

    private float enemyReactionTime = -1f;
    private bool fightEnded;
    private bool drawStarted;
    
    public bool IsActionPaused { get; private set; } = false;
    

    [Header("Controllers")]
    [SerializeField] private EnemyController currentEnemyController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RewardSystemController rewardSystemController;
    [SerializeField] private TimingQTE timingQTE;
    private TutorialControllerFastDraw tutorialControllerFastDraw;
    public static event System.Action OnQTEStarted;
    public static event System.Action OnQTEReset;

    protected override void Awake()
    {
        base.Awake();
        tutorialControllerFastDraw = GetComponent<TutorialControllerFastDraw>();
    }

    protected void Start()
    {
        if(tutorialControllerFastDraw.IsInTutorial())
            StartDraw();
    }

    private void Reset()
    {
        fightEnded = false;
        playerStreak = 0;
        longestPlayerStreak = 0;
        flawlessGame = true;
        fastestDrawTime = -1f;
    }
    public void StartDraw()
    {
        if (fightEnded || drawStarted) return;
        drawStarted = true;
        activeDrawSequenceRoutine = StartCoroutine(DrawSequence());
    }
    
    public void StopDraw()
    {
        if (activeDrawSequenceRoutine != null)
        {
            StopCoroutine(activeDrawSequenceRoutine);
            activeDrawSequenceRoutine = null;
        }

        canShoot = false;
        hasResult = false;
        drawStarted = false;

        UIManager.Instance.SetDrawText(false); // Hide the draw banner
    }

    private IEnumerator DrawSequence()
    {
        hasResult = false;
        canShoot = false;
        float waitTime = Random.Range(minDrawTime, maxDrawTime);
        yield return new WaitForSeconds(waitTime);

        UIManager.Instance.SetDrawText(true);
        canShoot = true;
        drawStarted = false;
        drawStartTime = Time.time;
        OnDrawSignal?.Invoke();
    }

    public void PlayerShot()
    {
        if (IsActionPaused) return;
        
        if (!canShoot)
        {
            Debug.Log("Player tried to shoot before draw signal.");
            PlayerShootAnimation();
            OnFiredEarly?.Invoke();
        
            if (activeDrawSequenceRoutine != null)
            {
                StopCoroutine(activeDrawSequenceRoutine);
                activeDrawSequenceRoutine = null;
            }

            return;
        }

        float reactionTime = Time.time - drawStartTime;

        // Player was slower — enemy wins
        if (reactionTime > enemyReactionTime && !IsPassiveEnemy())
        {
            Debug.Log("Player was slower than the enemy. No QTE triggered.");
            canShoot = false;
            DetermineFirstShooter(false); // Enemy wins
            return;
        }

        // Player was faster — trigger QTE
        canShoot = false;
        PauseAction();

        if (fastestDrawTime == -1f || reactionTime < fastestDrawTime)
            fastestDrawTime = reactionTime;

        timingQTE.PrepareIntroState();
        timingQTE.OnQTEComplete += HandleQTEResult;

        OnQTEStarted?.Invoke();

        timingQTE.PlayQTEIntro();
    }

    private void HandleQTEResult(QTEResult result)
    {
        Debug.LogError("Handle result");
        ResumeAction();
        timingQTE.OnQTEComplete -= HandleQTEResult;

        if (result == QTEResult.Good || result == QTEResult.Perfect)
        {
            Debug.LogError("Player shot");
            float reactionTime = Time.time - drawStartTime;

            if (fastestDrawTime == -1f || reactionTime < fastestDrawTime)
                fastestDrawTime = reactionTime;

            PlayerShootAnimation();
            DetermineFirstShooter(true);
        }
        else
        {
            Debug.Log("QTE failed: player missed");
            DetermineFirstShooter(false);
        }
    }

    public void SubscribeResultHandle()
    {
        timingQTE.OnQTEComplete -= HandleQTEResult;
        timingQTE.OnQTEComplete += HandleQTEResult;
    }

    public void PlayerShootAnimation()
    {
        playerController.Shoot();
    }
    
    public void DetermineFirstShooter(bool isPlayer = false)
    {
        if (hasResult) return;
        
        hasResult = true;

        if (isPlayer)
        {
            Debug.Log("Player was faster");
            playerStreak++;
            
            if (playerStreak > longestPlayerStreak)
            {
                longestPlayerStreak = playerStreak;
            }
        }
        else
        {
              flawlessGame = false;
              Debug.Log("Enemy was faster");
        }
        
        OnDrawResult?.Invoke(isPlayer);
        
        StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        if (activeDrawSequenceRoutine != null)
        {
            StopCoroutine(activeDrawSequenceRoutine);
            activeDrawSequenceRoutine = null;
        }
        
        yield return new WaitForSeconds(1f);
        
        UIManager.Instance.SetDrawText(false);
        StartDraw();
        
        OnQTEReset?.Invoke();
    }

    public void RoundEnd(bool playerWon)
    {
        UIManager.Instance.SetDrawText(false);
        Debug.Log("Ending round");
        fightEnded = true;
        
        if (activeDrawSequenceRoutine != null)
        {
            StopCoroutine(activeDrawSequenceRoutine);
            activeDrawSequenceRoutine = null;
        }
        
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
    
    public void PauseAction()
    {
        Debug.Log("Pause action");
        IsActionPaused = true;
    }

    public void ResumeAction()
    {
        Debug.Log("Resume action");
        IsActionPaused = false;
    }
    
    public void SetEnemyReactionTime(float reactionTime)
    {
        enemyReactionTime = reactionTime;
    }

    public bool IsPassiveEnemy()
    {
        return currentEnemyController.isPassiveEnemy;
    }
}
 