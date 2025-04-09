using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogueManager : SingletonMonoBehaviour<DialogueManager>
{
    [SerializeField]
    private RectTransform dialogueRectTransform;
    public AudioSource textAudioSource;
    public AudioClip textAudioClip;
    
    [SerializeField]
    public TextMeshProUGUI npcNameLabel;
    [SerializeField]
    public TextMeshProUGUI dialogueTextLabel;
    [SerializeField]
    public AudioSource npcAudioSource;
    
    [SerializeField]
    public ConversationScriptableObject currentConversation;
    private int currentDialogueIndex;
    public event Action OnEndConversation;


    //Dialogue Animation
    private const float RectTransformEndPositionX = 0f;
    private const float RectTransformStartPositionX = -1500f;
    private const float FadeTimer = 1f;
    
    public bool IsInConversation { get; private set; }
    public bool HasAllowedPlayerInteraction { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        StartConversation();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            NextDialogue();
        }
    }

    public void SetCurrentConversation(ConversationScriptableObject conversationScriptableObject)
    {
        currentConversation = conversationScriptableObject;
    }

    public void StartConversation(bool allowPlayerInteraction = true)
    {
        if (IsInConversation)
        {
            StartCoroutine(EndConversation());
        }
        
        ClearFields();
        currentDialogueIndex = 0;
        dialogueRectTransform.DOAnchorPosX(RectTransformEndPositionX , FadeTimer);
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
            //PlayerManager.Instance.SetCanInteract(true);
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
        dialogueRectTransform.DOAnchorPosX(RectTransformStartPositionX, FadeTimer);
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
        npcNameLabel.text = currentConversation.dialogueScriptableObjects[dialogueIndex].npcName;
        yield return new WaitForSeconds(textDelay);

        if (currentConversation.dialogueScriptableObjects[dialogueIndex].audioClip != null)
        {
            npcAudioSource.PlayOneShot(currentConversation.dialogueScriptableObjects[dialogueIndex].audioClip);
        }
        
        dialogueTextLabel.text = currentConversation.dialogueScriptableObjects[dialogueIndex].dialogue;
    }

    private void ClearFields()
    {
        npcNameLabel.text = string.Empty;
        dialogueTextLabel.text = string.Empty;
    }

    public void PlayTextAudioClip()
    {
        textAudioSource.PlayOneShot(textAudioClip);
    }
    
}
