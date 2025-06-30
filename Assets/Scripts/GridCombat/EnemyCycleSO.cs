using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyCycle", menuName = "Enemy/Enemy Cycle")]
public class EnemyCycleSO : ScriptableObject
{
    public List<EnemyCycleStep> steps = new();
}