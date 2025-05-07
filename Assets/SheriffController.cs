using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SheriffController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer counterSprite;
    public SpriteRenderer sheriffSprite;

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float slideDuration = 1f;
    public float sheriffFadeDelay = 0.2f;
    public Vector3 counterOffscreenOffset = new Vector3(0f, -5f, 0f);

    [Header("Button UI")]
    public RectTransform actionButton;
    public float buttonSlideDistance = 300f;
    public float buttonSlideDuration = 0.75f;

    [Header("Conversation")]
    [SerializeField] private ConversationScriptableObject headOutConversation;

    private Vector3 counterTargetPosition;
    private Vector2 buttonTargetPosition;
    private bool isReturning = false;

    private void Start()
    {
        counterTargetPosition = counterSprite.transform.position;

        if (actionButton != null)
        {
            buttonTargetPosition = actionButton.anchoredPosition;
            actionButton.anchoredPosition = buttonTargetPosition - new Vector2(0, buttonSlideDistance);
        }

        counterSprite.transform.position = counterTargetPosition + counterOffscreenOffset;
        counterSprite.color = SetAlpha(counterSprite.color, 0f);
        sheriffSprite.color = SetAlpha(sheriffSprite.color, 0f);

        RunSequence();
    }

    private void OnEnable()
    {
        DialogueManager.OnEndConversation += HandleEndConversation;
    }

    private void OnDisable()
    {
        DialogueManager.OnEndConversation -= HandleEndConversation;
    }

    private void HandleEndConversation()
    {
        if (DialogueManager.Instance.GetCurrentConversation == headOutConversation)
        {
            LoadScene(1);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && !isReturning)
        {
            LoadScene(0);
        }
    }

    private void RunSequence()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(counterSprite.transform.DOMove(counterTargetPosition, slideDuration).SetEase(Ease.OutQuad));
        sequence.Join(counterSprite.DOFade(1f, slideDuration));

        sequence.AppendInterval(sheriffFadeDelay);
        sequence.Append(sheriffSprite.DOFade(1f, fadeDuration));
    }

    private void LoadScene(int sceneIndex)
    {
        if (isReturning) return;
        isReturning = true;
        GameManager.Instance.LoadScene(sceneIndex, fadeDuration, FadeType.Simple);
    }

    public void OnWantedPosterButton()
    {
        DialogueManager.Instance.SetCurrentConversation(headOutConversation, true);
        SlideOutWantedPoster();
    }

    public void SlideInWantedPoster()
    {
        if (actionButton != null)
        {
            actionButton.DOAnchorPosY(
                buttonTargetPosition.y,
                buttonSlideDuration
            ).SetEase(Ease.OutBack);
        }
    }

    public void SlideOutWantedPoster()
    {
        if (actionButton != null)
        {
            actionButton.DOAnchorPosY(
                buttonTargetPosition.y - buttonSlideDistance,
                buttonSlideDuration
            ).SetEase(Ease.InBack);
        }
    }

    private Color SetAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
