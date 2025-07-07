using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public bool showGrid = true;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || GridManager.Instance == null || !showGrid) return;

        Gizmos.color = Color.green;
        for (int x = 0; x < GridManager.Instance.columns; x++)
        {
            for (int y = 0; y < GridManager.Instance.rows; y++)
            {
                Vector3 pos = GridManager.Instance.GridToWorld(new Vector2Int(x, y));
                Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
            }
        }
    }
}