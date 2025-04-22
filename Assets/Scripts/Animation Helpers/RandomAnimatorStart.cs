using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimatorStart : MonoBehaviour
{
    [Header("Start Time Offset")]
    [Tooltip("Randomizes the starting playback time of the default animation")]
    public bool randomizeTimeOffset = true;

    [Tooltip("Maximum normalized time offset (0 = start, 1 = end of animation cycle)")]
    [Range(0f, 1f)] public float maxOffset = 1f;

    [Header("Optional")]
    [Tooltip("Delay before the animation starts (helps offset triggering animations)")]
    public bool delayStart = false;
    public float minDelay = 0f;
    public float maxDelay = 0.5f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f; // Freeze until we offset manually
    }

    private void Start()
    {
        if (delayStart)
        {
            float delay = Random.Range(minDelay, maxDelay);
            Invoke(nameof(StartWithOffset), delay);
        }
        else
        {
            StartWithOffset();
        }
    }

    private void StartWithOffset()
    {
        if (randomizeTimeOffset)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float offset = Random.Range(0f, maxOffset);
            animator.Play(state.fullPathHash, 0, offset);
        }

        animator.speed = 1f;
    }
}