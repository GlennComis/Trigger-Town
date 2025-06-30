using System.Collections.Generic;
using UnityEngine;

public static class DontDestroyOnLoadManager
{
    private static readonly HashSet<GameObject> registered = new();

    public static void MarkDontDestroy(GameObject go)
    {
        if (!registered.Contains(go))
        {
            Object.DontDestroyOnLoad(go);
            registered.Add(go);
        }
    }
}