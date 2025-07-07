using UnityEngine;

[System.Serializable]
public class EnemyCycleStep
{ 
    [Header("Generic")]  
    public EnemyCycleActionType actionType;
    public float waitTime;
    
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public int damage = 1;
    public float projectileSpeed = 5f;
    public int range = 4;
    public WeaponTargetingPattern pattern;
    
    [Header("Movement")]
    public GridDirection moveDirection;
    
    [Header("Pathfinding")]
    public Vector2Int targetTile; // for PathToTile
}

public enum EnemyCycleActionType
{
    Wait,
    Move,
    Shoot,
    SeekPlayer,
    PathToTile
}