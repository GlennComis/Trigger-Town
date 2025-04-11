using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SheriffController : MonoBehaviour
{
    [Header("References")]
    public Image loadingScreenImage; // The UI image for the loading screen
    public SpriteRenderer counterSprite;
    public SpriteRenderer sheriffSprite;

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float slideDuration = 1f;
    public float sheriffFadeDelay = 0.2f;
    public Vector3 counterOffscreenOffset = new Vector3(0f, -5f, 0f);

    [Header("Button UI")]
    public RectTransform actionButton; // The button to animate
    public float buttonSlideDistance = 300f;
    public float buttonSlideDuration = 0.75f;

    [Header("Conversation")]
    [SerializeField]private ConversationScriptableObject headOutConversation;

    private Vector3 counterTargetPosition;
    private Vector2 buttonTargetPosition;
    private bool isReturning = false;

    private void Start()
    {
        // Save final positions
        counterTargetPosition = counterSprite.transform.position;

        if (actionButton != null)
        {
            buttonTargetPosition = actionButton.anchoredPosition;
            actionButton.anchoredPosition = buttonTargetPosition - new Vector2(0, buttonSlideDistance);
        }

        // Setup visuals
        counterSprite.transform.position = counterTargetPosition + counterOffscreenOffset;
        counterSprite.color = SetAlpha(counterSprite.color, 0f);
        sheriffSprite.color = SetAlpha(sheriffSprite.color, 0f);
        loadingScreenImage.color = SetAlpha(loadingScreenImage.color, 1f);

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
            LoadSceneWithFade(1);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && !isReturning)
        {
            isReturning = true;
            FadeAndLoadScene(0);
        }
    }

    private void RunSequence()
    {
        Sequence sequence = DOTween.Sequence();

        // Fade out loading screen
        sequence.Append(loadingScreenImage.DOFade(0f, fadeDuration));

        // Slide and fade in counter
        sequence.Append(counterSprite.transform.DOMove(counterTargetPosition, slideDuration).SetEase(Ease.OutQuad));
        sequence.Join(counterSprite.DOFade(1f, slideDuration));

        // Fade in sheriff
        sequence.AppendInterval(sheriffFadeDelay);
        sequence.Append(sheriffSprite.DOFade(1f, fadeDuration));
        
    }

    private void LoadSceneWithFade(int index)
    {
        //todo: trigger dialogue on button and after that fade when conversation is complete
        if (isReturning) return;
        isReturning = true;

        Sequence sequence = DOTween.Sequence();

        // Fade screen to black
        sequence.AppendCallback(() =>
        {
            loadingScreenImage.color = SetAlpha(loadingScreenImage.color, 0f);
        });
        sequence.Append(loadingScreenImage.DOFade(1f, fadeDuration));

        // Load scene
        sequence.AppendCallback(() =>
        {
            SceneManager.LoadScene(index);
        });
    }

    public void OnWantedPosterButton()
    {
        DialogueManager.Instance.SetCurrentConversation(headOutConversation, true);
        SlideOutWantedPoster();
    }

    private void FadeAndLoadScene(int index)
    {
        loadingScreenImage.color = SetAlpha(loadingScreenImage.color, 0f);
        loadingScreenImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(index);
        });
    }

    private Color SetAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
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
}
