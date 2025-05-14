using UnityEngine;

public class PlayerController : CharacterController
{
    protected override void Awake()
    {
        base.Awake();
        SetupCharacter(PlayerManager.Instance.maxHealth, string.Empty);
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
            PlayerManager.Instance.currentHealth = currentHealth;
            
            if (UIManager.instance_exists)
            {
                UIManager.Instance.SetHealth();
            }else{
                Debug.LogError("UI Manager does not exist");
            }
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