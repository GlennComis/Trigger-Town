using System.Collections;
using UnityEngine;

public class TutorialControllerFastDraw : MonoBehaviour, ISaveable
{
    [Header("Conversations")]
    public ConversationScriptableObject fastDrawTutorialConversation;
    public ConversationScriptableObject qteDialogueConversation;
    public ConversationScriptableObject earlyFireDialogueConversation;
    public ConversationScriptableObject missedQTEConversation;
    public ConversationScriptableObject completedQTE;

    private int currentStep = 0;
    private bool hasCompleted = false;
    private bool hasShownQteText;
    public string SaveKey => SaveKeys.TutorialFastDraw;
    public bool isInTutorial;

    private void Awake()
    {
        if (GameManager.instance_exists)
            GameManager.Instance.Load();
        else
            this.enabled = false;
    }

    private void Start()
    {
        if (!hasCompleted)
            StartCoroutine(StartTutorial());
    }

    private void OnEnable()
    {
        DialogueManager.OnDialogueShown += HandleDialogueTrigger;
        DialogueManager.OnEndConversation += HandleEndConversation;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueShown -= HandleDialogueTrigger;
        DialogueManager.OnEndConversation -= HandleEndConversation;
    }

    public object CaptureData() => new TutorialFastDrawData { hasCompleted = hasCompleted };

    public void RestoreData(object data)
    {
        if (data is TutorialFastDrawData saved)
            hasCompleted = saved.hasCompleted;
    }

    private IEnumerator StartTutorial()
    {
        isInTutorial = true;
        DialogueManager.Instance.SetCurrentConversation(fastDrawTutorialConversation, true);
        yield break;
    }

    private void HandleDialogueTrigger(DialogueScriptableObject dialogue)
    {
        if (!fastDrawTutorialConversation.dialogueScriptableObjects.Contains(dialogue)) return;
        if (dialogue != fastDrawTutorialConversation.dialogueScriptableObjects[currentStep]) return;

        currentStep++;
    }

    private void HandleEndConversation()
    {
        var current = DialogueManager.Instance.GetCurrentConversation;

        if (current == fastDrawTutorialConversation)
        {
            if (currentStep == 6)
            {
                
            }
        }
        else if (current == earlyFireDialogueConversation)
        {
        }
        else if (current == qteDialogueConversation)
        {
        }
        else if (current == missedQTEConversation)
        {
        }
        else if (current == completedQTE && !hasCompleted)
        {
            hasCompleted = true;
            isInTutorial = false;
        }
    }
    

    private IEnumerator FinishTutorial()
    {
        yield return new WaitForSeconds(0.5f);
        DialogueManager.Instance.SetCurrentConversation(completedQTE, true);
    }

    private void HandleOnFiredEarly()
    {
        HUDManager.Instance.HideCountdown();
        DialogueManager.Instance.SetCurrentConversation(earlyFireDialogueConversation, true);
    }

    private void HandleOnQTEReset()
    {
        
    }

    public bool IsInTutorial() => isInTutorial;
}