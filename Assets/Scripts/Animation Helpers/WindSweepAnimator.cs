using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSweepAnimator : MonoBehaviour
{
    [Header("Grass Animation Targets")]
    [Tooltip("List of GameObjects with Animator components")]
    public List<Animator> grassAnimators = new List<Animator>();

    [Header("Wind Settings")]
    [Tooltip("Delay in seconds between each grass animation")]
    public float delayBetween = 0.1f;

    [Tooltip("Whether to start automatically on Start")]
    public bool playOnStart = true;

    [Tooltip("Optional delay before wind begins")]
    public float windStartDelay = 0f;

    private void Awake()
    {
        // Disable all animations at startup
        foreach (var animator in grassAnimators)
        {
            if (animator != null)
            {
                animator.speed = 0f;
            }
        }
    }

    private void Start()
    {
        if (playOnStart)
            StartCoroutine(PlayWindSweep());
    }

    public void TriggerWindSweep()
    {
        StartCoroutine(PlayWindSweep());
    }

    private IEnumerator PlayWindSweep()
    {
        if (windStartDelay > 0)
            yield return new WaitForSeconds(windStartDelay);

        foreach (var animator in grassAnimators)
        {
            if (animator == null) continue;

            animator.speed = 1f;
            animator.Play(0, 0, 0f); // Play from beginning
            yield return new WaitForSeconds(delayBetween);
        }
    }
}