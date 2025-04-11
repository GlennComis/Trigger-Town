using System;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField]
    private SaveManager saveManager;

    protected override void Awake()
    {
        saveManager.Load();
        base.Awake();
        DontDestroyOnLoadManager.MarkDontDestroy(this.gameObject);
    }
    
    
}