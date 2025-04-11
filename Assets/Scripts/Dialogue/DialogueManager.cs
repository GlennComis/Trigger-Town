using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Febucci.UI;
using Random = UnityEngine.Random;

public class DialogueManager : SingletonMonoBehaviour<DialogueManager>
{
    [Header("UI")]
    [SerializeField] private RectTransform dialogueRectTransform;
    [SerializeField] private TextMeshProUGUI npcNameLabel;
    [SerializeField] private TextMeshProUGUI dialogueTextLabel;
    
    [SerializeField] private TextAnimator_TMP textAnimator;
    
    [Header("Audio")]
    [SerializeField] private AudioSource npcAudioSource;
    [SerializeField] private AudioSource textAudioSource;
    [SerializeField] private AudioClip textBlipClip;
    [SerializeField] private int blipFrequency = 3; // Play sound every N letters
    private float pitchVariation = 0.03f;

    private ConversationScriptableObject currentConversation;
    public ConversationScriptableObject GetCurrentConversation => currentConversation;
    private int currentDialogueIndex;

    public static event Action<DialogueScriptableObject> OnDialogueShown;

    [Header("Dialogue Animation")]
    private const float RectTransformEndPositionY = 150f;
    private const float RectTransformStartPositionY = -100f;
    private const float FadeTimer = 1f;
    
    public bool IsInConversation { get; private set; }
    public bool HasAllowedPlayerInteraction { get; private set; }
    
    public static event Action OnEndConversation;
    
    private void Update()
    {
        if (!IsInConversation) return;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (!textAnimator.allLettersShown)
            {
                textAnimator.SetVisibilityEntireText(true, false);
            }
            else
            {
                NextDialogue();
            }
        }
    }

    public void SetCurrentConversation(ConversationScriptableObject conversationScriptableObject, bool autoStartConversation = false)
    {
        currentConversation = conversationScriptableObject;
        
        if(autoStartConversation)
            StartConversation();
    }

    public void StartConversation(bool allowPlayerInteraction = true)
    {
        if (IsInConversation)
        {
            StartCoroutine(EndConversation());
        }

        ClearFields();
        currentDialogueIndex = 0;
        dialogueRectTransform.DOAnchorPosY(RectTransformEndPositionY, FadeTimer);
        StartCoroutine(SetDialogue(currentDialogueIndex, FadeTimer));
        IsInConversation = true;

        HasAllowedPlayerInteraction = allowPlayerInteraction;
    }

    private void NextDialogue()
    {
        if (!IsInConversation) return;

        currentDialogueIndex++;

        if (currentDialogueIndex > currentConversation.dialogueScriptableObjects.Count - 1)
        {
            StartCoroutine(EndConversation());
            return;
        }

        StartCoroutine(SetDialogue(currentDialogueIndex));
    }

    public void DoEndConversation()
    {
        if (!IsInConversation)
            return;

        StartCoroutine(EndConversation());
    }

    private IEnumerator EndConversation()
    {
        dialogueRectTransform.DOAnchorPosY(RectTransformStartPositionY, FadeTimer);
        yield return new WaitForSeconds(FadeTimer);

        currentDialogueIndex = 0;
        IsInConversation = false;

        if (!HasAllowedPlayerInteraction)
        {
            HasAllowedPlayerInteraction = true;
        }

        OnEndConversation?.Invoke();
    }

    private IEnumerator SetDialogue(int dialogueIndex, float textDelay = 0f)
    {
        DialogueScriptableObject dialogueData = currentConversation.dialogueScriptableObjects[dialogueIndex];

        npcNameLabel.text = dialogueData.npcName;
        yield return new WaitForSeconds(textDelay);

        if (dialogueData.audioClip != null)
        {
            npcAudioSource.PlayOneShot(dialogueData.audioClip);
        }

        textAnimator.textFull = dialogueData.dialogue;
        
        OnDialogueShown?.Invoke(dialogueData);
    }

    private void ClearFields()
    {
        npcNameLabel.text = string.Empty;
        dialogueTextLabel.text = string.Empty;
    }

    private int characterCounter = 0;

    public void OnCharacterPrinted()
    {
        characterCounter++;

        if (characterCounter % blipFrequency == 0)
        {
            textAudioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
            textAudioSource.PlayOneShot(textBlipClip);
        }
    }
}
