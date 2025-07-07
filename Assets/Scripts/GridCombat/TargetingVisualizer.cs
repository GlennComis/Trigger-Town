using System.Collections.Generic;
using UnityEngine;

public class TargetingVisualizer : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private PlayerGridMover player;

    private readonly List<GameObject> activeIndicators = new();

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        ClearIndicators();

        if (weaponController == null || weaponController.currentWeapon == null || player == null)
            return;

        List<Vector2Int> tilePositions = TargetingSystem.GetTargetTiles(
            weaponController.currentWeapon,
            player.gridPosition,
            isPlayer: true
        );

        foreach (var gridPos in tilePositions)
        {
            // Only show indicators on the ENEMY side of the grid
            if (gridPos.x < GridManager.Instance.columns)
                continue;

            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);
            GameObject indicator = Instantiate(indicatorPrefab, worldPos, Quaternion.identity, indicatorParent);
            activeIndicators.Add(indicator);
        }
    }

    private void ClearIndicators()
    {
        foreach (var indicator in activeIndicators)
        {
            Destroy(indicator);
        }
        activeIndicators.Clear();
    }
}