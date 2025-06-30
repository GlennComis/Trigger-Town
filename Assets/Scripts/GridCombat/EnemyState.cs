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
        enemy.NotifyStepComplete();
    }

    public void Update() { }
    public void Exit() { }
}

public class MoveState : IEnemyState
{
    private EnemyAIController enemy;

    public void Enter(EnemyAIController enemy)
    {
        this.enemy = enemy;

        Vector2Int direction = enemy.nextMoveDirection;
        if (direction == Vector2Int.zero)
        {
            Debug.LogWarning($"{enemy.name} has no direction set for MoveState.");
            enemy.TransitionToState(EnemyStateType.Idle);
            return;
        }

        Vector2Int newPos = enemy.mover.gridPosition + direction;

        if (GridManager.Instance.IsWithinBounds(newPos) &&
            !GridManager.Instance.IsTileOccupied(newPos))
        {
            if (enemy.mover.TryMoveTo(newPos))
            {
                enemy.NotifyStepComplete();
            }
        }
        else
        {
            Debug.Log($"{enemy.name} cannot move to {newPos}. Transitioning to Idle.");
            enemy.TransitionToState(EnemyStateType.Idle);
        }
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
        enemy.NotifyStepComplete();
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
        enemy.NotifyStepComplete();
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
        enemy.NotifyStepComplete();
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
        enemy.NotifyStepComplete();
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
        enemy.NotifyStepComplete();
    }

    public void Update() { }
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
            enemy.NotifyStepComplete();
            return;
        }

        Vector2Int start = enemy.mover.gridPosition;
        Vector2Int target = enemy.targetTile;

        if (start == target)
        {
            enemy.NotifyStepComplete();
            return;
        }

        path = GridManager.Instance.FindPath(start, target);

        if (path.Count > 0 && path.Peek() == start)
        {
            path.Dequeue();
        }

        if (path.Count == 0)
        {
            Debug.Log($"{enemy.name} has no valid path to target.");
            enemy.NotifyStepComplete();
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
            enemy.NotifyStepComplete();
            return;
        }

        Vector2Int nextTile = path.Dequeue();
        bool moved = enemy.mover.TryMoveTo(nextTile);

        if (!moved)
        {
            Debug.LogWarning($"{enemy.name} failed to move to {nextTile}, aborting path.");
            enemy.NotifyStepComplete();
        }
    }

    public void Exit()
    {
        path.Clear();
    }
}