using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyGridMover))]
public class EnemyAIController : CharacterController, IStatefulCharacter
{
    [Header("Combat Settings")]
    [SerializeField] private Vector3 projectileSpawnOffset = new Vector3(-0.5f, 0f, 0f);

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
    [HideInInspector] public Vector2Int nextMoveDirection = Vector2Int.zero;

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

    [Header("Scripted Cycle")]
    public EnemyCycleSO behaviorCycle;
    private int currentCycleIndex = 0;
    private float cycleTimer = 0f;
    private bool waitingForStepCompletion = false;

    protected override void Awake()
    {
        base.Awake();

        idleState = new IdleState();
        moveState = new MoveState();
        attackState = new AttackState();
        tookDamageState = new TookDamageState();
        lowHealthState = new LowHealthState();
        healState = new HealState();
        stunnedState = new StunnedState();
        seekPlayerState = new SeekPlayerState();
        pathfindToTileState = new PathfindToTileState();

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

        if (behaviorCycle != null && currentCycleIndex < behaviorCycle.steps.Count)
        {
            ProcessCycle();
            return;
        }

        if (stateTimer >= minStateDuration)
        {
            EvaluateTransitions();
        }
    }

    public void TransitionToState(EnemyStateType newStateType)
    {
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
    
    public override void Shoot()
    {
        base.Shoot();

        if (behaviorCycle == null || currentCycleIndex >= behaviorCycle.steps.Count)
            return;

        var step = behaviorCycle.steps[currentCycleIndex];

        if (step.projectilePrefab == null)
        {
            Debug.LogWarning($"{name} tried to shoot but has no projectile assigned.");
            return;
        }

        Vector3 shootWorldDirection = Vector2.left;

        GameObject projectileGO = Instantiate(step.projectilePrefab, transform.position + projectileSpawnOffset, Quaternion.identity);
        Projectile proj = projectileGO.GetComponent<Projectile>();
        proj.direction = shootWorldDirection;
        proj.speed = step.projectileSpeed;
        proj.damage = step.damage;
        proj.maxDistance = step.range;

        if (projectileGO.TryGetComponent(out SpriteRenderer sr))
            sr.flipX = true;
    }
    
    public void MoveToNextPosition()
    {
        Debug.Log($"{name} moves!");
        mover.TryMove();
    }

    public void MoveInDirection(GridDirection direction)
    {
        Vector2Int dir = direction.ToVector();

        if (dir == Vector2Int.zero)
        {
            Debug.LogWarning($"{name} tried to move in an invalid direction.");
            return;
        }

        nextMoveDirection = dir;
        TransitionToState(EnemyStateType.Move);
    }

    public void SeekPlayer()
    {
        Debug.Log($"{name} seeks player!");
        mover.SeekPlayer();
    }

    public void PerformAttack()
    {
        Shoot();
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

    public void NotifyStepComplete()
    {
        waitingForStepCompletion = false;
        cycleTimer = 0f;
        AdvanceCycle();
    }

    private void ProcessCycle()
    {
        if (mover.isMoving || waitingForStepCompletion)
            return;

        if (currentCycleIndex >= behaviorCycle.steps.Count)
            return;

        EnemyCycleStep step = behaviorCycle.steps[currentCycleIndex];
        cycleTimer += Time.deltaTime;

        if (cycleTimer < step.waitTime)
            return;

        waitingForStepCompletion = true;

        switch (step.actionType)
        {
            case EnemyCycleActionType.Wait:
                NotifyStepComplete();
                break;
            case EnemyCycleActionType.Move:
                MoveInDirection(step.moveDirection);
                break;
            case EnemyCycleActionType.Shoot:
                PerformAttack();
                NotifyStepComplete();
                break;
            case EnemyCycleActionType.SeekPlayer:
                SeekPlayer();
                NotifyStepComplete();
                break;
            case EnemyCycleActionType.PathToTile:
                if (mover.gridPosition != step.targetTile)
                {
                    targetTile = step.targetTile;
                    TransitionToState(EnemyStateType.PathfindToTile);
                }
                else
                {
                    NotifyStepComplete();
                }
                break;
        }
    }

    private void AdvanceCycle()
    {
        currentCycleIndex = (currentCycleIndex + 1) % behaviorCycle.steps.Count;
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + projectileSpawnOffset, 0.1f);
    }
}