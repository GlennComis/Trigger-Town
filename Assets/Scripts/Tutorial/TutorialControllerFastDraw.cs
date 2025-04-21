using System;
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

    [Header("References")]
    [SerializeField] private FastDrawManager fastDrawManager;
    [SerializeField] private TimingQTE timingQTE;

    private int currentStep = 0;
    private bool hasCompleted = false;
    private bool hasShownQteText;
    public string SaveKey => SaveKeys.TutorialFastDraw;
    public bool isInTutorial;

    private void Awake()
    {
        GameManager.Instance.Load();
    }

    private void Start()
    {
        if (!hasCompleted)
            StartCoroutine(StartTutorial());
    }

    private void OnEnable()
    {
        FastDrawManager.OnFiredEarly += HandleOnFiredEarly;
        FastDrawManager.OnQTEReset += HandleOnQTEReset;
        
        DialogueManager.OnDialogueShown += HandleDialogueTrigger;
        DialogueManager.OnEndConversation += HandleEndConversation;
        
        timingQTE.OnQTEStarted += HandleQTEStart;
        timingQTE.OnQTEComplete += HandleQTEResult;
    }

    private void OnDisable()
    {
        FastDrawManager.OnFiredEarly -= HandleOnFiredEarly;
        FastDrawManager.OnQTEReset -= HandleOnQTEReset;
        DialogueManager.OnDialogueShown -= HandleDialogueTrigger;
        DialogueManager.OnEndConversation -= HandleEndConversation;
        
        timingQTE.OnQTEStarted -= HandleQTEStart;
        timingQTE.OnQTEComplete -= HandleQTEResult;
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
        fastDrawManager.PauseAction();
        DialogueManager.Instance.SetCurrentConversation(fastDrawTutorialConversation, true);
        yield break;
    }

    private void HandleDialogueTrigger(DialogueScriptableObject dialogue)
    {
        if (!fastDrawTutorialConversation.dialogueScriptableObjects.Contains(dialogue)) return;
        if (dialogue != fastDrawTutorialConversation.dialogueScriptableObjects[currentStep]) return;

        switch (currentStep)
        {
           
        }

        currentStep++;
    }

    private void HandleEndConversation()
    {
        if (DialogueManager.Instance.GetCurrentConversation == fastDrawTutorialConversation)
        {
           switch (currentStep)
                   {
                       case 6:
                           fastDrawManager.ResumeAction();
                           fastDrawManager.StartDraw();
                           break;
                   } 
        }
        else if (DialogueManager.Instance.GetCurrentConversation == earlyFireDialogueConversation)
        {
            fastDrawManager.ResumeAction();
            fastDrawManager.StartDraw();
        }
        else if (DialogueManager.Instance.GetCurrentConversation == qteDialogueConversation)
        {
            timingQTE.ResumeQTE();
        }
    }

    private void HandleQTEStart()
    {
        if (hasShownQteText) return;
        hasShownQteText = true;
        fastDrawManager.PauseAction();
        timingQTE.PauseQTE();

        // Show QTE explanation dialogue
        DialogueManager.Instance.SetCurrentConversation(qteDialogueConversation, true);
    }

    private void HandleQTEResult(QTEResult result)
    {
        if (hasCompleted) return;

        if (result == QTEResult.Miss)
        {
            StartCoroutine(HandleQTERetry());
        }
        else
        {
            StartCoroutine(FinishTutorial());
        }
    }

    private IEnumerator HandleQTERetry()
    {
        timingQTE.OnQTEComplete -= HandleQTEResult;

        yield return null; //Wait a single frame to prevent timing issue
        fastDrawManager.PauseAction();
        yield return new WaitForSeconds(1f);

        DialogueManager.Instance.SetCurrentConversation(missedQTEConversation, true);
        
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInConversation);

        timingQTE.OnQTEComplete += HandleQTEResult;
        timingQTE.PlayQTEIntro();
    }
    
    private void HandleOnFiredEarly()
    {
        fastDrawManager.StopDraw();
        fastDrawManager.PauseAction();
        DialogueManager.Instance.SetCurrentConversation(earlyFireDialogueConversation, true);
    }
    
    private void HandleOnQTEReset()
    {
        Debug.Log("Prevents the user from getting the banner again");
        fastDrawManager.StopDraw();
    }

    private IEnumerator FinishTutorial()
    {
        isInTutorial = false;
        timingQTE.OnQTEComplete -= HandleQTEResult;

        yield return new WaitForSeconds(0.5f);
        DialogueManager.Instance.SetCurrentConversation(completedQTE, true);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInConversation);

        hasCompleted = true;
        fastDrawManager.ResumeAction();
    }

    public bool IsInTutorial()
    {
        return isInTutorial;
    }
}
