using System.Collections;
using UnityEngine;

public class EnemyController : CharacterController
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyData enemyData;

    private float reactionTime;
    private bool canShoot = false;
    private bool hasShot = false;
 
    private Coroutine shootRoutine;
    private readonly WaitForSeconds playerFiredEarlyDelay = new WaitForSeconds(0.5f);

    private void OnEnable()
    {
        FastDrawManager.OnDrawSignal += PrepareToShoot;
        FastDrawManager.OnDrawResult += HandleResult;
        FastDrawManager.OnFiredEarly += OnPlayerFiredEarly;
    }

    private void OnDisable()
    {
        FastDrawManager.OnDrawSignal -= PrepareToShoot;
        FastDrawManager.OnDrawResult -= HandleResult;
        FastDrawManager.OnFiredEarly -= OnPlayerFiredEarly;
    }

    protected override void Awake()
    {
        base.Awake();
        SetupCharacter(enemyData.maxHealth, enemyData.enemyName);
    }

    private void PrepareToShoot()
    {
        if (enemyData.IsBoss())
        {
            // Optional: boss intro logic or delay here
        }

        if (enemyData.isPassive)
            return;

        canShoot = true;
        hasShot = false;
        reactionTime = Random.Range(enemyData.minReactionTime, enemyData.maxReactionTime);

        FastDrawManager.Instance.SetEnemyReactionTime(reactionTime);

        if (shootRoutine != null)
            StopCoroutine(shootRoutine);

        shootRoutine = StartCoroutine(ShootAfterDelay());
    }

    private IEnumerator ShootAfterDelay()
    {
        yield return new WaitForSeconds(reactionTime);

        if (!hasShot && !FastDrawManager.Instance.isActionPaused)
        {
            Shoot();
            hasShot = true;
            FastDrawManager.Instance.DetermineFirstShooter();
        }
    }

    private void OnPlayerFiredEarly()
    {
        if (enemyData.isPassive)
            return;

        hasShot = true;
        StartCoroutine(PlayerFiredEarlyRoutine());
    }

    private IEnumerator PlayerFiredEarlyRoutine()
    {
        yield return playerFiredEarlyDelay;

        if (!FastDrawManager.Instance.isActionPaused)
        {
            Shoot();
            FastDrawManager.Instance.DetermineFirstShooter();
        }
    }

    private void HandleResult(bool playerWon)
    {
        /*if (playerWon)
            TakeDamage();
        else
            Shoot();*/
        

        canShoot = false;
        hasShot = true;
    }

    protected override void Die()
    {
        base.Die();
        FastDrawManager.Instance.RoundEnd(true);
    }

    public EnemyData GetEnemyData()
    {
        return enemyData;
    }

    public bool IsPassiveEnemy()
    {
        return enemyData.isPassive;
    }

    public int GetReward()
    {
        return enemyData.bountyReward;
    }
    
    public int GetXpReward()
    {
        return enemyData.xpReward;
    }
}
