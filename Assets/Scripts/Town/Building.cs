using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Building : MonoBehaviour, IEnterable
{
    [Header("Scene Loading")]
    public int sceneBuildIndex;
    public bool isOpen = true;

    [Header("Closed Dialogue")]
    public ConversationScriptableObject closedConversation;

    [Header("Fade Overlay")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Door Animation (optional)")]
    public Animator doorAnimator;
    public string doorOpenTrigger = "Open";

    private bool isTransitioning = false;

    private void Awake()
    {
        if (doorAnimator != null)
            doorAnimator.speed = 0f; // Prevent auto-play
    }

    public void EnterBuilding()
    {
        if (isTransitioning) return;

        if (!isOpen)
        {
            if (closedConversation != null && !DialogueManager.Instance.IsInConversation)
            {
                DialogueManager.Instance.SetCurrentConversation(closedConversation, true);
            }
            else
            {
                Debug.Log("Building is closed. No closed conversation assigned.");
            }
            return;
        }

        isTransitioning = true;
        Debug.Log($"Entering building: {gameObject.name}", gameObject);

        // Play door animation if available
        if (doorAnimator != null)
        {
            doorAnimator.speed = 1f;
            if (!string.IsNullOrEmpty(doorOpenTrigger))
                doorAnimator.SetTrigger(doorOpenTrigger);
        }

        // Fade and load scene
        fadeImage.raycastTarget = true;
        fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneBuildIndex);
        });
    }
}