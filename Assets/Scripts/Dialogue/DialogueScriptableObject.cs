using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue", order = 2)]
public class DialogueScriptableObject : ScriptableObject
{
    [Header("Dialogue")]
    public string npcName;
    public Sprite sprite;
    public string dialogue;

    [Header("Audio")]
    public AudioClip audioClip;

    [Header("Interaction")]
    public bool preventPlayerInteraction = true;
}