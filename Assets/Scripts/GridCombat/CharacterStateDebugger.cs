using System.Collections.Generic;
using UnityEngine;

public class CharacterStateDebugger : MonoBehaviour
{
    private bool showDebugger = false;

    private readonly List<IStatefulCharacter> trackedCharacters = new();

    private Vector2 scrollPosition;

    private void Start()
    {
        RefreshCharacterList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showDebugger = !showDebugger;
        }
    }

    private void RefreshCharacterList()
    {
        trackedCharacters.Clear();
        IStatefulCharacter[] found = FindObjectsOfType<MonoBehaviour>(true) as IStatefulCharacter[];
        foreach (var c in found)
        {
            if (c != null)
                trackedCharacters.Add(c);
        }
    }

    private void OnGUI()
    {
        if (!showDebugger) return;

        GUI.Box(new Rect(10, 10, 300, 400), "Character State Debugger");

        GUILayout.BeginArea(new Rect(20, 40, 280, 360));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var character in trackedCharacters)
        {
            GUILayout.Label($"{character.CharacterName} → {character.CurrentState}");
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Refresh List"))
        {
            RefreshCharacterList();
        }

        GUILayout.EndArea();
    }
}