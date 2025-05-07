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

        currentStep++;
    }

    private void HandleEndConversation()
    {
        var current = DialogueManager.Instance.GetCurrentConversation;

        if (current == fastDrawTutorialConversation)
        {
            if (currentStep == 6)
            {
                fastDrawManager.ResumeAction();
                fastDrawManager.StartDraw();
            }
        }
        else if (current == earlyFireDialogueConversation)
        {
            fastDrawManager.ResumeAction();
            fastDrawManager.StartDraw();
        }
        else if (current == qteDialogueConversation)
        {
            timingQTE.ResumeQTE();
        }
        else if (current == missedQTEConversation)
        {
            fastDrawManager.ResetRoundStateForRetry();
            fastDrawManager.SubscribeResultHandle();
            timingQTE.PlayQTEIntro();
        }
        else if (current == completedQTE && !hasCompleted)
        {
            hasCompleted = true;
            isInTutorial = false;
            fastDrawManager.RoundEnd(true);
        }
    }

    private void HandleQTEStart()
    {
        if (hasShownQteText) return;
        hasShownQteText = true;
        fastDrawManager.PauseAction();
        timingQTE.PauseQTE();

        DialogueManager.Instance.SetCurrentConversation(qteDialogueConversation, true);
    }

    private void HandleQTEResult(QTEResult result)
    {
        if (hasCompleted) return;

        if (result == QTEResult.Miss)
        {
            fastDrawManager.PlayerShootAnimation();
            StartCoroutine(HandleQTERetry());
        }
        else
        {
            StartCoroutine(FinishTutorial());
        }
    }

    private IEnumerator HandleQTERetry()
    {
        yield return null; // Wait a single frame
        fastDrawManager.PauseAction();
        HUDManager.Instance.HideCountdown();
        yield return new WaitForSeconds(1f);

        DialogueManager.Instance.SetCurrentConversation(missedQTEConversation, true);
    }

    private IEnumerator FinishTutorial()
    {
        yield return new WaitForSeconds(0.5f);
        fastDrawManager.PauseAction();
        DialogueManager.Instance.SetCurrentConversation(completedQTE, true);
    }

    private void HandleOnFiredEarly()
    {
        fastDrawManager.StopDraw();
        fastDrawManager.PauseAction();
        HUDManager.Instance.HideCountdown();
        DialogueManager.Instance.SetCurrentConversation(earlyFireDialogueConversation, true);
    }

    private void HandleOnQTEReset()
    {
        
    }

    public bool IsInTutorial() => isInTutorial;
}