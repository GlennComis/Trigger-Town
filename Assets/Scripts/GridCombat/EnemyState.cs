using System.Collections.Generic;
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
    PathfindToTile,
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

public class PathfindToTileState : IEnemyState
{
    private EnemyAIController enemy;
    private Queue<Vector2Int> path = new();
    private float waitTimer = 0f;

    private const float StepDelay = 1.0f;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;
        waitTimer = 0f;

        if (enemy.mover == null || GridManager.Instance == null)
        {
            Debug.LogWarning("Pathfinding aborted: missing mover or GridManager.");
            return;
        }

        Vector2Int start = enemy.mover.gridPosition;
        Vector2Int target = enemy.targetTile;

        // Generate A* path
        path = GridManager.Instance.FindPath(start, target);

        // Skip the current tile
        if (path.Count > 0 && path.Peek() == start)
        {
            path.Dequeue();
        }

        if (path.Count == 0)
        {
            Debug.Log($"{enemy.name} has no valid path to target.");
        }
    }

    public void Update()
    {
        if (enemy.mover.isMoving)
            return;

        waitTimer += Time.deltaTime;

        if (waitTimer < StepDelay)
            return;

        waitTimer = 0f;

        if (path.Count == 0)
        {
            // Destination reached
            enemy.TransitionToState(EnemyStateType.Idle);
            return;
        }

        Vector2Int nextTile = path.Dequeue();
        bool moved = enemy.mover.TryMoveTo(nextTile);

        if (!moved)
        {
            Debug.LogWarning($"{enemy.name} failed to move to {nextTile}, aborting path.");
            enemy.TransitionToState(EnemyStateType.Idle);
        }
    }

    public void Exit()
    {
        path.Clear();
    }
}


