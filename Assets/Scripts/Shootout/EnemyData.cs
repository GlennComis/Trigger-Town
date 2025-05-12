using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "TriggerTown/Enemy")]
public class EnemyData : ScriptableObject
{
    #region Identity

    [Header("Identity")]
    public string enemyName;
    public Sprite portraitIcon;

    #endregion

    #region Combat Behavior

    [Header("Combat")]
    public bool isOneShotKill;
    public bool isPassive;
    public QTEType qteType;
    public float minReactionTime = 0.3f;
    public float maxReactionTime = 0.5f;

    #endregion

    #region Stats

    [Header("Stats")]
    public int maxHealth = 1;
    public int bountyReward = 10;
    public int xpReward = 100;
    public EnemyRank rank;

    #endregion

    #region Visual & Sound

    [Header("Audio")]
    public List<AudioClip> gunshotClips;
    public Vector2 pitchRange = new Vector2(0.8f, 1.1f);

    #endregion

    #region Helpers

    public bool IsBoss()
    {
        return rank == EnemyRank.Boss;
    }

    #endregion
}


public enum EnemyRank
{
    Common,
    Rare,
    Elite,
    Boss
}