using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyController : CharacterController
{
    [SerializeField] private float minReactionTime = 0.3f;
    [SerializeField] private float maxReactionTime = 0.5f;
    public bool isPassiveEnemy;
    private float reactionTime;
    private bool canShoot = false;
    private bool hasShot = false;
    private WaitForSeconds playerFiredEarlyReactionDelay = new WaitForSeconds(.5f);

    private Coroutine shootRoutine;

    private void OnEnable()
    {
        FastDrawManager.OnDrawSignal += PrepareToShoot;
        FastDrawManager.OnDrawResult += ProcessResult;
        FastDrawManager.OnFiredEarly += PlayerFiredEarly;
    }

    private void OnDisable()
    {
        FastDrawManager.OnDrawSignal -= PrepareToShoot;
        FastDrawManager.OnDrawResult -= ProcessResult;
        FastDrawManager.OnFiredEarly -= PlayerFiredEarly;
    }

    private void PrepareToShoot()
    {
        if (isPassiveEnemy) return;
        reactionTime = Random.Range(minReactionTime, maxReactionTime);
        FastDrawManager.Instance.SetEnemyReactionTime(reactionTime);
        canShoot = true;
        hasShot = false;

        if (shootRoutine != null)
            StopCoroutine(shootRoutine);

        shootRoutine = StartCoroutine(ShootAfterDelay());
    }

    private void ProcessResult(bool playerWon)
    {
        if (playerWon)
        {
            TakeDamage();
        }

        hasShot = true;
        canShoot = false;
    }

    private void PlayerFiredEarly()
    {
        if (isPassiveEnemy) return;
        hasShot = true;
        StartCoroutine(PlayerFiredEarlyRoutine());
    }

    private IEnumerator PlayerFiredEarlyRoutine()
    {
        yield return playerFiredEarlyReactionDelay;

        if (!FastDrawManager.Instance.IsActionPaused)
        {
            Shoot();
            FastDrawManager.Instance.DetermineFirstShooter();
        }
    }

    private IEnumerator ShootAfterDelay()
    {
        yield return new WaitForSeconds(reactionTime);

        // Stop if logic is paused or already shot
        if (hasShot || FastDrawManager.Instance.IsActionPaused)
            yield break;

        Shoot();
        hasShot = true;
        FastDrawManager.Instance.DetermineFirstShooter();
    }

    protected override void Die()
    {
        base.Die();
        FastDrawManager.Instance.RoundEnd(true);
    }
}
