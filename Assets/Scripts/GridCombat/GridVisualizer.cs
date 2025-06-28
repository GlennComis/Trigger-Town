using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public bool showPlayerGrid = true;
    public bool showEnemyGrid = true;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || GridManager.Instance == null) return;

        Gizmos.color = Color.green;
        for (int x = 0; x < GridManager.Instance.columns; x++)
        {
            for (int y = 0; y < GridManager.Instance.rows; y++)
            {
                if (showPlayerGrid)
                {
                    Vector3 pos = GridManager.Instance.GetWorldPosition(new Vector2Int(x, y), true);
                    Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
                }

                if (showEnemyGrid)
                {
                    Vector3 pos = GridManager.Instance.GetWorldPosition(new Vector2Int(x, y), false);
                    Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
                }
            }
        }
    }
}