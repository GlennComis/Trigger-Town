using UnityEngine;
using DG.Tweening;

public class TrainController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform endPoint;
    public float moveSpeed = 2f;

    [Header("Timing Settings")]
    public float minDelay = 5f;
    public float maxDelay = 10f;

    private Vector3 startPos;
    private Vector3 originalScale;

    void Start()
    {
        startPos = transform.position;
        originalScale = transform.localScale;

        StartNextRide();
    }

    void StartNextRide()
    {
        float delay = Random.Range(minDelay, maxDelay);
        Invoke(nameof(StartRide), delay);
    }

    void StartRide()
    {
        bool goingToEnd = Random.value > 0.5f;

        Vector3 from = goingToEnd ? startPos : endPoint.position;
        Vector3 to = goingToEnd ? endPoint.position : startPos;

        // Move train to the correct start point in case it's out of place
        transform.position = from;

        float distance = Vector3.Distance(from, to);
        float duration = distance / moveSpeed;

        // Flip the train
        Vector3 newScale = originalScale;
        newScale.x = goingToEnd ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
        transform.localScale = newScale;

        transform.DOMove(to, duration)
            .SetEase(Ease.Linear)
            .OnComplete(StartNextRide);
    }
}