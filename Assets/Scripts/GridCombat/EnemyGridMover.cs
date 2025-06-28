using UnityEngine;

public class EnemyGridMover : MonoBehaviour
{
    public float moveSpeed = 4f;
    public Vector2Int gridPosition = new Vector2Int(3, 1);
    public Vector2Int previousPosition = new Vector2Int(-1, -1);
    public Vector3 positionOffset = Vector3.zero;

    private Vector3 targetWorldPosition;
    public bool isMoving = false;

    private float moveCooldown = 2f;
    private float moveTimer = 0f;

    private void Start()
    {
        targetWorldPosition = GridManager.Instance.GetWorldPosition(gridPosition, false) + positionOffset;
        transform.position = targetWorldPosition;
        GridManager.Instance.RegisterEnemy(gridPosition, gameObject);
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
        {
            transform.position = targetWorldPosition;
            GridManager.Instance.MoveEnemy(previousPosition, gridPosition);
            isMoving = false;
        }
    }

    public void TryMove()
    {
        Vector2Int[] directions = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = 0; i < 10; i++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int newGridPos = gridPosition + dir;

            if (GridManager.Instance.IsWithinBounds(newGridPos) &&
                !GridManager.Instance.IsTileOccupied(newGridPos))
            {
                previousPosition = gridPosition;
                gridPosition = newGridPos;
                targetWorldPosition = GridManager.Instance.GetWorldPosition(gridPosition, false) + positionOffset;
                isMoving = true;
                break;
            }
        }
    }

    public void SeekPlayer()
    {
        Vector2Int playerPos = GridManager.Instance.GetClosestPlayerGridPosition(gridPosition);

        Vector2Int bestDir = Vector2Int.zero;
        float bestDistance = float.MaxValue;

        Vector2Int[] directions = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = gridPosition + dir;
            if (!GridManager.Instance.IsWithinBounds(newPos)) continue;
            if (GridManager.Instance.IsTileOccupied(newPos)) continue;

            float dist = (playerPos - newPos).sqrMagnitude;
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestDir = dir;
            }
        }

        if (bestDir != Vector2Int.zero)
        {
            previousPosition = gridPosition;
            gridPosition += bestDir;
            targetWorldPosition = GridManager.Instance.GetWorldPosition(gridPosition, false) + positionOffset;
            isMoving = true;
        }
    }
}
