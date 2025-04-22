using UnityEngine;
using DG.Tweening;

public class AmbientMover : MonoBehaviour
{
    public enum LoopMode
    {
        Reverse,   // Ping-pong style
        ResetToStart // Teleport back to start after reaching the end
    }

    [Header("Movement")]
    public Transform endPoint;
    public float moveSpeed = 2f;
    public Ease movementEase = Ease.Linear;

    [Header("Timing")]
    public float minDelay = 3f;
    public float maxDelay = 6f;

    [Header("Behavior")]
    public LoopMode loopMode = LoopMode.Reverse;
    public bool allowFlipping = true;

    private Vector3 startPos;
    private Vector3 originalScale;
    private bool goingToEnd = true;

    private void Start()
    {
        if (endPoint == null)
        {
            Debug.LogWarning($"{gameObject.name} has no endPoint set.", this);
            return;
        }

        startPos = transform.position;
        originalScale = transform.localScale;

        StartNextMove();
    }

    private void StartNextMove()
    {
        float delay = Random.Range(minDelay, maxDelay);
        Invoke(nameof(BeginMove), delay);
    }

    private void BeginMove()
    {
        Vector3 from = goingToEnd ? startPos : endPoint.position;
        Vector3 to = goingToEnd ? endPoint.position : startPos;

        // Reset position if required
        if (loopMode == LoopMode.ResetToStart && !goingToEnd)
            transform.position = startPos;

        float distance = Vector3.Distance(from, to);
        float duration = distance / moveSpeed;

        // Flip sprite if needed
        if (allowFlipping)
        {
            Vector3 newScale = originalScale;
            newScale.x = goingToEnd ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
            transform.localScale = newScale;
        }

        transform.DOMove(to, duration)
            .SetEase(movementEase)
            .OnComplete(() =>
            {
                goingToEnd = !goingToEnd;
                StartNextMove();
            });
    }
}
