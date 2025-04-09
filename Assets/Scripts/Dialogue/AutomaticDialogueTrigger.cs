using UnityEngine;

public class AutomaticDialogueTrigger: MonoBehaviour
{
    public bool hasTriggered;
    public bool dialogueEndTrigger;
    
    [SerializeField]
    private ConversationScriptableObject conversationScriptableObject;

    private void OnEnable()
    {
        TriggerDialogue();
    }

    private void TriggerDialogue()
    {
        if (dialogueEndTrigger)
        {
            DialogueManager.Instance.DoEndConversation();
            return;
        }
        
        if (hasTriggered)
            return;
        
        
        hasTriggered = true;
        DialogueManager.Instance.SetCurrentConversation(conversationScriptableObject);
        DialogueManager.Instance.StartConversation();
    }
}