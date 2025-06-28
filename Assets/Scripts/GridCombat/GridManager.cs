using System.Collections.Generic;
using UnityEngine;

public class GridManager : SingletonMonoBehaviour<GridManager>
{
    [Header("Grid Settings")]
    public int columns = 4;
    public int rows = 4;
    public Vector2 tileSize = new Vector2(1, 1);
    public Vector2 origin = new Vector2(-4, -2); // Bottom-left origin

    [Header("Tile Visuals")]
    public Sprite tileSprite;
    public Color playerTileColor = Color.blue;
    public Color enemyTileColor = Color.red;
    public Transform tileParent; // Optional, for scene hierarchy cleanliness
    
    
    private Dictionary<Vector2Int, GameObject> enemyGridMap = new();
    
    protected override void Awake()
    {
        base.Awake();
        GenerateGrids();
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos, bool isPlayer)
    {
        float xOffset = isPlayer ? 0 : columns * tileSize.x + 1f;
        return new Vector3(
            origin.x + gridPos.x * tileSize.x + xOffset,
            origin.y + gridPos.y * tileSize.y,
            0f
        );
    }

    public bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < columns && pos.y >= 0 && pos.y < rows;
    }

    private void GenerateGrids()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Player tile
                CreateTile(pos, true, playerTileColor);

                // Enemy tile
                CreateTile(pos, false, enemyTileColor);
            }
        }
    }

    private void CreateTile(Vector2Int gridPos, bool isPlayer, Color color)
    {
        Vector3 worldPos = GetWorldPosition(gridPos, isPlayer);

        // Optional tile spacing (set this to ~0.1f or adjust as needed)
        float spacing = 0.1f;
        Vector3 scaledSize = new Vector3(tileSize.x - spacing, tileSize.y - spacing, 1f);

        GameObject tileGO = new GameObject($"{(isPlayer ? "Player" : "Enemy")}Tile_{gridPos.x}_{gridPos.y}");
        tileGO.transform.position = worldPos;
        tileGO.transform.localScale = scaledSize;

        if (tileParent != null)
            tileGO.transform.parent = tileParent;

        var sr = tileGO.AddComponent<SpriteRenderer>();
        sr.sprite = tileSprite;
        sr.color = color;
        sr.sortingOrder = 5;
    }

    public void RegisterEnemy(Vector2Int gridPosition, GameObject enemy)
    {
        if (!enemyGridMap.ContainsKey(gridPosition))
        {
            enemyGridMap.Add(gridPosition, enemy);
        }
    }

    public void MoveEnemy(Vector2Int oldPos, Vector2Int newPos)
    {
        if (enemyGridMap.TryGetValue(oldPos, out GameObject enemy))
        {
            enemyGridMap.Remove(oldPos);
            enemyGridMap[newPos] = enemy;
        }
    }

    public void UnregisterEnemy(Vector2Int pos)
    {
        if (enemyGridMap.ContainsKey(pos))
        {
            enemyGridMap.Remove(pos);
        }
    }

    public GameObject GetEnemyAt(Vector2Int pos)
    {
        enemyGridMap.TryGetValue(pos, out GameObject enemy);
        return enemy;
    }

    public bool IsTileOccupied(Vector2Int pos)
    {
        return enemyGridMap.ContainsKey(pos);
    }

}