using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Conversation", menuName = "ScriptableObjects/Conversation", order = 1)]
public class ConversationScriptableObject : ScriptableObject
{
    [Header("The dialogue is player in order of the List arrangement")]
    public List<DialogueScriptableObject> dialogueScriptableObjects;
}