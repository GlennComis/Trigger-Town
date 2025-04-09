using System;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public bool hasTriggered;
    public bool dialogueEndTrigger;
    
    [SerializeField]
    private ConversationScriptableObject conversationScriptableObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
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
}