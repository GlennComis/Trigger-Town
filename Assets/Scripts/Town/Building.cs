using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Building : MonoBehaviour, IEnterable
{
    [Header("Scene Loading")]
    public int sceneBuildIndex;

    [Header("Fade Overlay")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Door Animation")]
    [Tooltip("Animator controlling the door opening")]
    public Animator doorAnimator;

    [Tooltip("Name of the animation state or trigger to play")]
    public string doorOpenTrigger = "Open";

    private bool isTransitioning = false;

    public void EnterBuilding()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        Debug.Log($"Entering building: {gameObject.name}", gameObject);

        // Play door animation
        if (doorAnimator != null)
        {
            doorAnimator.speed = 1f; // unpause if needed

            // Use trigger if set
            if (!string.IsNullOrEmpty(doorOpenTrigger))
            {
                doorAnimator.SetTrigger(doorOpenTrigger);
            }
        }

        // Fade to black and load the scene
        fadeImage.raycastTarget = true;
        fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneBuildIndex);
        });
    }
}