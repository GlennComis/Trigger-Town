using UnityEngine;

public class PlayerController : CharacterController
{
    private void OnEnable()
    {
        GameManager.OnDrawResult += ProcessResult;
    }
    
    private void OnDisable()
    {
        GameManager.OnDrawResult -= ProcessResult;
    }

    private void ProcessResult(bool playerWon)
    {
        if (!playerWon)
        {
            TakeDamage();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.PlayerShot();
        }
    }

    protected override void Die()
    {
        base.Die();
        GameManager.Instance.RoundEnd(false);
    }
}