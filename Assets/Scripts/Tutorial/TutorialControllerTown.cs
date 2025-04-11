using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialControllerTown : MonoBehaviour, ISaveable
{
    [SerializeField]
    private List<SpriteRenderer> buildings;
    [SerializeField]
    private TutorialOverlayController tutorialOverlayController;
    
    [Header("Conversations")]
    public ConversationScriptableObject townIntroductionSheriff;
    public string SaveKey => SaveKeys.TutorialTown;
    private bool hasCompleted;
    private int currentStep;

    private void Start()
    {
        if(!hasCompleted)
            StartTutorial();
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
        return new TutorialTownData
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

        if (data is TutorialTownData saved)
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
        if (!townIntroductionSheriff.dialogueScriptableObjects.Contains(dialogue))
            return;
        
        if (dialogue == townIntroductionSheriff.dialogueScriptableObjects[currentStep])
        {
            Debug.Log($"Tutorial step {currentStep} triggered");

            switch (currentStep)
            {
                case 3: 
                    tutorialOverlayController.ShowFirstHighlight(buildings[0]);
                    break;
                case 5:
                    tutorialOverlayController.HighlightNext(buildings[1]);
                    break;
                case 7:
                    tutorialOverlayController.HighlightNext(buildings[2]);
                    break;
                case 9:
                    tutorialOverlayController.HideOverlay(); 
                    break;
            }

            currentStep++;
        }
    }
    
    private void HandleEndConversation()
    {
        hasCompleted = true;
        TownManager.Instance.EnableBuildingSelection();
        TownManager.Instance.EnableArrowInstanceGameObject(); 
        Debug.Log("Town Tutorial completed");
    }
    
    // Track dialogue state to focus on buildings while being in a certain dialogue window
    // Enable interaction for the town selection
    // Push state to the next enum in the GameManager
    private void StartTutorial()
    {
        StartCoroutine(DisableArrow());
        TownManager.Instance.DisableBuildingSelection();
        DialogueManager.Instance.SetCurrentConversation(townIntroductionSheriff, true);
    }

    private static IEnumerator DisableArrow()
    {
        yield return TownManager.Instance.ArrowInstanceExists();
        TownManager.Instance.DisableArrowInstanceGameObject();
    }
}