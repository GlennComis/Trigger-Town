using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyGridMover))]
public class EnemyAIController : CharacterController, IStatefulCharacter
{
    private IEnemyState currentState;
    private EnemyStateType currentStateType;

    private IdleState idleState;
    private MoveState moveState;
    private AttackState attackState;
    private TookDamageState tookDamageState;
    private LowHealthState lowHealthState;
    private HealState healState;
    private StunnedState stunnedState;
    private SeekPlayerState seekPlayerState;
    private PathfindToTileState pathfindToTileState;

    [Header("State Settings")]
    public EnemyGridMover mover;

    public float attackCooldown = 3f;
    private float attackTimer;

    public bool isStunned;

    private float stateTimer;
    public float minStateDuration = 1f;

    [Range(0f, 1f)]
    public float aggressionLevel = 0.5f;

    public Vector2Int targetTile;

    public EnemyStateType CurrentStateType => currentStateType;
    public string CharacterName => name;
    public string CurrentState => currentStateType.ToString();

    protected override void Awake()
    {
        base.Awake();

        // Initialize state instances
        idleState = new IdleState();
        moveState = new MoveState();
        attackState = new AttackState();
        tookDamageState = new TookDamageState();
        lowHealthState = new LowHealthState();
        healState = new HealState();
        stunnedState = new StunnedState();
        seekPlayerState = new SeekPlayerState();
        pathfindToTileState = new PathfindToTileState();

        // Cache mover
        mover = GetComponent<EnemyGridMover>();
    }

    private void Start()
    {
        TransitionToState(EnemyStateType.Idle);
        attackTimer = attackCooldown;
        stateTimer = 0f;
    }

    private void Update()
    {
        currentState?.Update();
        stateTimer += Time.deltaTime;

        if (stateTimer >= minStateDuration)
        {
            EvaluateTransitions();
        }
    }

    public void TransitionToState(EnemyStateType newStateType)
    {
        if (currentStateType == newStateType)
            return;

        currentState?.Exit();

        currentStateType = newStateType;
        currentState = GetStateInstance(newStateType);

        minStateDuration = GetMinDurationForState(newStateType);
        stateTimer = 0f;

        currentState?.Enter(this);
    }

    private IEnemyState GetStateInstance(EnemyStateType type)
    {
        return type switch
        {
            EnemyStateType.Idle => idleState,
            EnemyStateType.Move => moveState,
            EnemyStateType.Attack => attackState,
            EnemyStateType.TookDamage => tookDamageState,
            EnemyStateType.LowHealth => lowHealthState,
            EnemyStateType.Heal => healState,
            EnemyStateType.Stunned => stunnedState,
            EnemyStateType.SeekPlayer => seekPlayerState,
            EnemyStateType.PathfindToTile => pathfindToTileState,
            _ => null
        };
    }

    private float GetMinDurationForState(EnemyStateType state)
    {
        return state switch
        {
            EnemyStateType.Idle => 0.75f,
            EnemyStateType.Move => .5f,
            EnemyStateType.Attack => .5f,
            EnemyStateType.TookDamage => .2f,
            EnemyStateType.LowHealth => .1f,
            EnemyStateType.Heal => .5f,
            EnemyStateType.Stunned => .1f,
            EnemyStateType.SeekPlayer => .5f,
            EnemyStateType.PathfindToTile => 1.5f,
            _ => 1f
        };
    }

    private void EvaluateTransitions()
    {
        if (isStunned)
        {
            TransitionToState(EnemyStateType.Stunned);
            return;
        }

        if (currentHealth <= maxHealth * 0.3f)
        {
            TransitionToState(EnemyStateType.LowHealth);
            return;
        }

        if (aggressionLevel > 0.7f && IsPlayerVisible() && currentStateType != EnemyStateType.SeekPlayer)
        {
            TransitionToState(EnemyStateType.SeekPlayer);
            return;
        }

        if (attackTimer <= 0f)
        {
            TransitionToState(EnemyStateType.Attack);
            attackTimer = attackCooldown;
            return;
        }

        attackTimer -= Time.deltaTime;

        if (!mover.isMoving)
        {
            TransitionToState(EnemyStateType.Move);
        }
        else
        {
            TransitionToState(EnemyStateType.Idle);
        }
    }

    private bool IsPlayerVisible()
    {
        Vector2Int closestPlayerPos = GridManager.Instance.GetClosestPlayerGridPosition(mover.gridPosition);
        if (closestPlayerPos.x == -1 && closestPlayerPos.y == -1)
            return false;

        return Vector2Int.Distance(mover.gridPosition, closestPlayerPos) < 4f;
    }

    // Public methods that states can call
    public void MoveToNextPosition()
    {
        Debug.Log($"{name} moves!");
        mover.TryMove();
    }

    public void SeekPlayer()
    {
        Debug.Log($"{name} seeks player!");
        mover.SeekPlayer();
    }

    public void PerformAttack()
    {
        Debug.Log($"{name} attacks!");
    }

    public void OnTookDamage()
    {
        Debug.Log($"{name} was hit!");
    }

    public void OnLowHealth()
    {
        Debug.Log($"{name} is in low health state!");
    }

    public void Heal()
    {
        Debug.Log($"{name} heals!)");
        currentHealth += 10;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public void Stun()
    {
        Debug.Log($"{name} Stunned!");
        isStunned = true;
        Invoke(nameof(ClearStun), 2f);
    }

    private void ClearStun()
    {
        isStunned = false;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        TransitionToState(EnemyStateType.TookDamage);
    }

    protected override void Die()
    {
        if (mover != null)
        {
            GridManager.Instance.UnregisterEnemy(mover.gridPosition);
        }

        base.Die();
    }
}