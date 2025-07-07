using System.Collections.Generic;
using UnityEngine;

public static class TargetingSystem
{
    public static List<Vector2Int> GetTargetTiles(WeaponData weapon, Vector2Int origin, bool isPlayer)
    {
        List<Vector2Int> results = new();

        switch (weapon.pattern)
        {
            case WeaponTargetingPattern.ForwardLine:
            case WeaponTargetingPattern.PiercingLine:
                {
                    int dir = isPlayer ? 1 : -1;
                    for (int i = 1; i <= weapon.range; i++)
                    {
                        Vector2Int tile = origin + new Vector2Int(i * dir, 0);
                        if (GridManager.Instance.IsWithinBounds(tile))
                            results.Add(tile);
                    }
                }
                break;

            case WeaponTargetingPattern.ConeSpread:
                {
                    int dir = isPlayer ? 1 : -1;
                    for (int i = 1; i <= weapon.range; i++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            Vector2Int tile = origin + new Vector2Int(i * dir, y);
                            if (GridManager.Instance.IsWithinBounds(tile))
                                results.Add(tile);
                        }
                    }
                }
                break;

            case WeaponTargetingPattern.ThrownExplosion:
                {
                    int dir = isPlayer ? 1 : -1;
                    Vector2Int center = origin + new Vector2Int(2 * dir, 0);

                    if (GridManager.Instance.IsWithinBounds(center))
                    {
                        List<Vector2Int> offsets = new()
                        {
                            Vector2Int.zero,
                            Vector2Int.up,
                            Vector2Int.down,
                            Vector2Int.left,
                            Vector2Int.right
                        };

                        foreach (var offset in offsets)
                        {
                            Vector2Int tile = center + offset;
                            if (GridManager.Instance.IsWithinBounds(tile))
                                results.Add(tile);
                        }
                    }
                }
                break;

            case WeaponTargetingPattern.ForwardStun:
                {
                    int dir = isPlayer ? 1 : -1;
                    for (int i = 1; i <= 2; i++)
                    {
                        Vector2Int tile = origin + new Vector2Int(i * dir, 0);
                        if (GridManager.Instance.IsWithinBounds(tile))
                            results.Add(tile);
                    }
                }
                break;
        }

        return results;
    }

    public static List<GameObject> GetTargets(WeaponData weapon, Vector2Int origin, bool isPlayer)
    {
        List<GameObject> hits = new();
        List<Vector2Int> targets = GetTargetTiles(weapon, origin, isPlayer);

        int hitsRemaining = weapon.maxPenetration > 0 ? weapon.maxPenetration : int.MaxValue;

        foreach (var pos in targets)
        {
            GameObject enemy = GridManager.Instance.GetEnemyAt(pos);
            if (enemy != null)
            {
                hits.Add(enemy);
                hitsRemaining--;
                if (hitsRemaining <= 0) break;
            }
        }

        return hits;
    }
}
