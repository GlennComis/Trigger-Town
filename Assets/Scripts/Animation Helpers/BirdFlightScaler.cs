using UnityEngine;
using DG.Tweening;

public class BirdFlightScaler : MonoBehaviour
{
    [Header("Scale Settings")]
    [Tooltip("Target X scale when bird is far away")]
    public float minScaleX = 0f;

    [Tooltip("Time (in seconds) to scale between states")]
    public float duration = 20f;

    [Header("Looping")]
    [Tooltip("Continuously loop the flight scaling back and forth")]
    public bool loopBack = true;

    [Header("Start Options")]
    [Tooltip("Start automatically when scene begins")]
    public bool playOnStart = true;

    private void Start()
    {
        if (playOnStart)
        {
            Invoke(nameof(TriggerFlight), 0);
        }
    }

    public void TriggerFlight()
    {
        transform.DOKill(); // Stop any existing tweens

        float targetX = minScaleX;

        // Start by shrinking X, then loop as desired
        Tween tween = transform.DOScaleX(targetX, duration)
            .SetEase(Ease.InOutSine);

        if (loopBack)
        {
            tween.SetLoops(-1, LoopType.Yoyo); // Loop forever back and forth
        }
    }
}