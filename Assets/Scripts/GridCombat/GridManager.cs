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
    
    public IReadOnlyList<PlayerGridMover> Players => playerList;
    private readonly List<PlayerGridMover> playerList = new();
    private Dictionary<Vector2Int, GameObject> enemyGridMap = new();
    
    protected override void Awake()
    {
        base.Awake();
        GenerateGrids();
    }

    #region TileCreation
    public Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / tileSize.x);
        int y = Mathf.FloorToInt((worldPos.y - origin.y) / tileSize.y);
        return new Vector2Int(x, y);
    }
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            origin.x + gridPos.x * tileSize.x + tileSize.x / 2f,
            origin.y + gridPos.y * tileSize.y + tileSize.y / 2f,
            0f
        );
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            origin.x + gridPos.x * tileSize.x + tileSize.x / 2f,
            origin.y + gridPos.y * tileSize.y + tileSize.y / 2f,
            0f
        );
    }

    public bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < columns * 2 && pos.y >= 0 && pos.y < rows;
    }

    public bool IsEnemySide(Vector2Int pos)
    {
        return pos.x >= columns;
    }

    public bool IsPlayerSide(Vector2Int pos)
    {
        return pos.x < columns;
    }

    private void GenerateGrids()
    {
        for (int x = 0; x < columns * 2; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                bool isPlayerSide = x < columns;
                Color color = isPlayerSide ? playerTileColor : enemyTileColor;
                CreateTile(pos, color);
            }
        }
    }

    private void CreateTile(Vector2Int gridPos, Color color)
    {
        Vector3 worldPos = GridToWorld(gridPos);

        // Optional tile spacing (set this to ~0.1f or adjust as needed)
        float spacing = 0.1f;
        Vector3 scaledSize = new Vector3(tileSize.x - spacing, tileSize.y - spacing, 1f);

        GameObject tileGO = new GameObject($"Tile_{gridPos.x}_{gridPos.y}");
        tileGO.transform.position = worldPos;
        tileGO.transform.localScale = scaledSize;

        if (tileParent != null)
            tileGO.transform.parent = tileParent;

        var sr = tileGO.AddComponent<SpriteRenderer>();
        sr.sprite = tileSprite;
        sr.color = color;
        sr.sortingOrder = 5;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / tileSize.x);
        int y = Mathf.FloorToInt((worldPos.y - origin.y) / tileSize.y);
        return new Vector2Int(x, y);
    }
    #endregion TileCreation

    #region Enemy
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
    #endregion Enemy
    
    #region Player
    public void RegisterPlayer(PlayerGridMover player)
    {
        if (!playerList.Contains(player)) 
        {
            playerList.Add(player);
        }
    }
    
    public void UnregisterPlayer(PlayerGridMover player)
    {
        if (playerList.Contains(player)) 
        {
            playerList.Remove(player);
        }
    }
    
    public Vector2Int GetClosestPlayerGridPosition(Vector2Int fromPos)
    {
        Vector2Int closest = new Vector2Int(-1, -1);
        float minDistance = float.MaxValue;

        foreach (var player in playerList)
        {
            float dist = Vector2Int.Distance(player.gridPosition, fromPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.gridPosition;
            }
        }

        return closest;
    }
    
    #endregion Player

    #region PathFinding
    
    public Queue<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var openSet = new PriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0f;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (IsTileOccupied(neighbor)) continue;

                float tentativeG = gScore[current] + 1; // All moves cost 1

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return new Queue<Vector2Int>(); // No path found
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (IsWithinBounds(neighbor))
                yield return neighbor;
        }
    }

    private Queue<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var totalPath = new List<Vector2Int> { current };

        while (cameFrom.TryGetValue(current, out Vector2Int previous))
        {
            current = previous;
            totalPath.Insert(0, current);
        }

        return new Queue<Vector2Int>(totalPath);
    }
    
    
    #endregion PathFinding
}