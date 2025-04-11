using System.Collections;
using UnityEngine;

public class TutorialControllerSheriff : MonoBehaviour, ISaveable
{
    [Header("Conversations")]
    public ConversationScriptableObject sheriffIntroduction;
    private const float SheriffAnimationWaitTime = 3.5f;

    [Header("Sheriff")]
    [SerializeField] private SheriffController sheriffController;
    
    public string SaveKey => SaveKeys.TutorialSheriff;
    private bool hasCompleted;
    private int currentStep;

    private void Start()
    {
        if(!hasCompleted)
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

    public object CaptureData()
    {
        return new TutorialSheriffData
        {
            hasCompleted = hasCompleted,
        };
    }

    public void RestoreData(object data)
    {
        if (data == null)
        {
            Debug.LogWarning($"No save data found for {SaveKey}, using defaults.");
            return;
        }

        if (data is TutorialSheriffData saved)
        {
            hasCompleted = saved.hasCompleted;
        }
        else
        {
            Debug.LogError($"Invalid data type for {SaveKey}: expected TutorialTownData.");
        }
    }
    
    private void HandleDialogueTrigger(DialogueScriptableObject dialogue)
    {
        if (!sheriffIntroduction.dialogueScriptableObjects.Contains(dialogue))
            return;
        
        if (dialogue == sheriffIntroduction.dialogueScriptableObjects[currentStep])
        {
            Debug.Log($"Tutorial step {currentStep} triggered");

            switch (currentStep)
            {
                
            }

            currentStep++;
        }
    }
    
    private void HandleEndConversation()
    {
        if(DialogueManager.Instance.GetCurrentConversation == sheriffIntroduction)
            sheriffController.SlideInWantedPoster();
    }
    
    private IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(SheriffAnimationWaitTime);
        DialogueManager.Instance.SetCurrentConversation(sheriffIntroduction, true);
    }
}