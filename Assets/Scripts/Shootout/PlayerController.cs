using UnityEngine;

public class PlayerController : CharacterController
{
    private int playerHealth = 1;
    protected override void Awake()
    {
        base.Awake();
        SetupCharacter(playerHealth, string.Empty);
    }

    private void OnEnable()
    {
        FastDrawManager.OnDrawResult += ProcessResult;
    }
    
    private void OnDisable()
    {
        FastDrawManager.OnDrawResult -= ProcessResult;
    }

    private void ProcessResult(bool playerWon)
    {
        if (!playerWon && !FastDrawManager.Instance.IsPassiveEnemy())
        {
            TakeDamage();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FastDrawManager.Instance.PlayerShot();
        }
    }

    protected override void Die()
    {
        base.Die();
        FastDrawManager.Instance.RoundEnd(false);
    }
}