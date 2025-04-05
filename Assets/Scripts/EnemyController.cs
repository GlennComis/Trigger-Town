using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : CharacterController
{
    [SerializeField] private float minReactionTime = 0.3f;
    [SerializeField] private float maxReactionTime = 0.5f;
    private float reactionTime;
    private bool canShoot = false;
    private bool hasShot = false;
    private WaitForSeconds playerFiredEarlyReactionDelay = new WaitForSeconds(.5f);

    private Coroutine shootRoutine;

    private void OnEnable()
    {
        GameManager.OnDrawSignal += PrepareToShoot;
        GameManager.OnDrawResult += ProcessResult;
        GameManager.OnFiredEarly += PlayerFiredEarly;
    }

    private void OnDisable()
    {
        GameManager.OnDrawSignal -= PrepareToShoot;
        GameManager.OnDrawResult -= ProcessResult;
        GameManager.OnFiredEarly -= PlayerFiredEarly;
    }

    private void PrepareToShoot()
    {
        reactionTime = Random.Range(minReactionTime, maxReactionTime);
        GameManager.Instance.SetEnemyReactionTime(reactionTime);
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
        hasShot = true;
        StartCoroutine(PlayerFiredEarlyRoutine());
    }

    private IEnumerator PlayerFiredEarlyRoutine()
    {
        yield return playerFiredEarlyReactionDelay;

        if (!GameManager.Instance.IsActionPaused)
        {
            Shoot();
            GameManager.Instance.DetermineFirstShooter();
        }
    }

    private IEnumerator ShootAfterDelay()
    {
        yield return new WaitForSeconds(reactionTime);

        // Stop if logic is paused or already shot
        if (hasShot || GameManager.Instance.IsActionPaused)
            yield break;

        Shoot();
        hasShot = true;
        GameManager.Instance.DetermineFirstShooter();
    }

    protected override void Die()
    {
        base.Die();
        GameManager.Instance.RoundEnd(true);
    }
}
