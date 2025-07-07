using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private PlayerGridMover player;
    public float speed = 5f;
    public int damage = 1;
    public Vector3 direction;
    public float maxDistance = 5f;

    private Vector3 startPos;
    private Vector2Int lastGridPos = new Vector2Int(-999, -999); // Initialize to invalid position

    private void Start()
    {
        startPos = transform.position;
        player = GridManager.Instance.Players.Count > 0 ? GridManager.Instance.Players[0] : null;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        Vector2Int currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);

        if (player == null)
        {
            if (GridManager.Instance.Players.Count > 0)
                player = GridManager.Instance.Players[0];
            else
                return;
        }

        Vector2Int playerGridPos = player.gridPosition;

        if (currentGridPos != lastGridPos)
        {
            lastGridPos = currentGridPos;
            Debug.Log($"[DEBUG] Projectile moved to {currentGridPos}, Player at {playerGridPos}");

            if (playerGridPos == currentGridPos)
            {
                Debug.Log("[DEBUG] Hit confirmed.");
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (Vector3.Distance(startPos, transform.position) > maxDistance)
            Destroy(gameObject);
    }
}