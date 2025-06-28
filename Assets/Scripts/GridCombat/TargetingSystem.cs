using System.Collections.Generic;
using UnityEngine;

public static class TargetingSystem
{
    private const int GridWidth = 4; // width of each grid (player/enemy)

    public static List<Vector2Int> GetTargetTiles(WeaponData weapon, Vector2Int origin, bool isPlayer)
    {
        List<Vector2Int> results = new();

        switch (weapon.pattern)
        {
            case WeaponTargetingPattern.ForwardLine:
            case WeaponTargetingPattern.PiercingLine:
                {
                    int startX = origin.x;
                    int endX = startX + weapon.range;

                    for (int i = startX + 1; i <= endX; i++)
                    {
                        if (i >= GridWidth && i < GridWidth * 2)
                        {
                            int enemyX = i - GridWidth;
                            results.Add(new Vector2Int(enemyX, origin.y));
                        }
                    }
                }
                break;

            case WeaponTargetingPattern.ConeSpread:
                {
                    for (int i = 1; i <= weapon.range; i++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            Vector2Int tile = origin + new Vector2Int(i, y);
                            if (tile.x >= GridWidth && tile.x < GridWidth * 2 &&
                                tile.y >= 0 && tile.y < GridManager.Instance.rows)
                            {
                                Vector2Int enemyTile = new Vector2Int(tile.x - GridWidth, tile.y);
                                results.Add(enemyTile);
                            }
                        }
                    }
                }
                break;

            case WeaponTargetingPattern.ThrownExplosion:
            {
                Vector2Int center = origin + Vector2Int.right * 2;

                // Only proceed if explosion center is on enemy grid
                if (center.x >= GridWidth && center.x < GridWidth * 2 && 
                    center.y >= 0 && center.y < GridManager.Instance.rows)
                {
                    Vector2Int enemyCenter = new Vector2Int(center.x - GridWidth, center.y);

                    // Valid directions from center
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
                        Vector2Int tile = enemyCenter + offset;

                        if (tile.x >= 0 && tile.x < GridWidth &&
                            tile.y >= 0 && tile.y < GridManager.Instance.rows)
                        {
                            results.Add(tile);
                        }
                    }
                }
            }
                break;


            case WeaponTargetingPattern.ForwardStun:
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        int virtualX = origin.x + i;
                        if (virtualX >= GridWidth && virtualX < GridWidth * 2)
                        {
                            int enemyX = virtualX - GridWidth;
                            results.Add(new Vector2Int(enemyX, origin.y));
                        }
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
