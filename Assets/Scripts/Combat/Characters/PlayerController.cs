using UnityEngine;

public class PlayerController : CharacterController
{
    private const int maxHealth = 100;
    protected override void Start()
    {
        SetupCharacter(maxHealth, string.Empty);
        base.Start();
    }
    

    private void OnEnable()
    {
        //FastDrawManager.OnDrawResult += ProcessResult;
    }
    
    private void OnDisable()
    {
        //FastDrawManager.OnDrawResult -= ProcessResult;
    }

    private void ProcessResult(bool playerWon)
    {
       
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        }
    }

    protected override void Die()
    {
        base.Die();
    }
}