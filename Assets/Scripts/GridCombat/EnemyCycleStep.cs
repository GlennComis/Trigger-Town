using UnityEngine;

[System.Serializable]
public class EnemyCycleStep
{
    public EnemyCycleActionType actionType;
    public float waitTime;

    public GridDirection moveDirection;
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