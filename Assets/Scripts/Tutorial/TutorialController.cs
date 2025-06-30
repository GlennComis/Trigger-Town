using System;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [SerializeField]
    private TutorialOverlayController tutorialOverlayController;

    private void Awake()
    {
        //tutorialOverlayController.ShowHighlight(target);
    }
}