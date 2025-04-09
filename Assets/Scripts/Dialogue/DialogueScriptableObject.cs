using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue", order = 2)]
public class DialogueScriptableObject : ScriptableObject
{
    public string npcName;
    
    public Sprite sprite;
    public string dialogue;

    public AudioClip audioClip;

    public bool preventPlayerInteraction = true;
}