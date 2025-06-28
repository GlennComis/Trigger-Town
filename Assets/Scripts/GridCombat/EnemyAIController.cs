using UnityEngine;

[RequireComponent(typeof(EnemyGridMover))]
public class EnemyAIController : MonoBehaviour
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

    [Header("State Settings")]
    public EnemyGridMover mover;

    public EnemyStateType CurrentStateType => currentStateType;

    private void Awake()
    {
        // Initialize state instances
        idleState = new IdleState();
        moveState = new MoveState();
        attackState = new AttackState();
        tookDamageState = new TookDamageState();
        lowHealthState = new LowHealthState();
        healState = new HealState();
        stunnedState = new StunnedState();

        // Cache mover
        mover = GetComponent<EnemyGridMover>();
    }

    private void Start()
    {
        TransitionToState(EnemyStateType.Idle);
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void TransitionToState(EnemyStateType newStateType)
    {
        if (currentStateType == newStateType)
            return;

        currentState?.Exit();

        currentStateType = newStateType;
        currentState = GetStateInstance(newStateType);

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
            _ => null
        };
    }

    // Public methods that states can call
    public void MoveToNextPosition()
    {
        Debug.Log($"{name} moves!");
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
    }

    public void Stun()
    {
        Debug.Log($"{name} Stunned!");
    }
}
