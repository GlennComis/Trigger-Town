using UnityEngine;

public interface IEnemyState
{
    void Enter(EnemyAIController enemy);
    void Update();
    void Exit();
}

public enum EnemyStateType
{
    Idle,
    Move,
    Attack,
    TookDamage,
    LowHealth,
    Heal,
    Stunned,
    SeekPlayer,
}

public class IdleState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
    }

    public void Update()
    {
        // Transition logic here
    }

    public void Exit() { }
}

public class MoveState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        enemy.MoveToNextPosition();
    }

    public void Update() { }

    public void Exit() { }
}

public class AttackState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        enemy.PerformAttack();
    }

    public void Update() { }

    public void Exit() { }
}

public class TookDamageState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        enemy.OnTookDamage();
    }

    public void Update() { }

    public void Exit() { }
}

public class LowHealthState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        enemy.OnLowHealth();
    }

    public void Update() { }

    public void Exit() { }
}

public class HealState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        enemy.Heal();
    }

    public void Update() { }

    public void Exit() { }
}

public class StunnedState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        enemy.Stun();
        
        Debug.Log($"{enemy.name} is stunned.");
    }
    
    public void Update() { }

    public void Exit()
    {
        Debug.Log($"{enemy.name} is no longer stunned.");
    }
}

public class SeekPlayerState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        Debug.Log($"{enemy.name} is seeking the player!");
        enemy.SeekPlayer();
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        Debug.Log($"{enemy.name} finished seeking.");
    }
}

