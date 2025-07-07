using UnityEngine;

public class PlayerGridMover : CharacterController
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Vector2Int gridPosition = new Vector2Int(0, 0);
    public Vector3 positionOffset = new Vector3(0f, -0.2f, 0f);

    [Header("Shooting")]
    public float shootCooldown = 0.5f;
    [SerializeField] private WeaponController weaponController;

    private Vector3 targetWorldPosition;
    private bool isMoving = false;
    private bool isShooting = false;
    private float shootTimer = 0f;
    
    protected override void Start()
    {
        base.Start();
        targetWorldPosition = GridManager.Instance.GetWorldPosition(gridPosition) + positionOffset;
        transform.position = targetWorldPosition;

        GridManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.UnregisterPlayer(this);
    }

    private void Update()
    {
        if (isShooting)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                isShooting = false;
            }
            return;
        }

        if (!isMoving && Input.GetKeyDown(KeyCode.Space))
        {
            if (weaponController.CanShoot)
            {
                weaponController.TryShoot(gridPosition);
                Shoot();
                isShooting = true;
                shootTimer = shootCooldown;
            }
            return;
        }

        if (!isMoving)
        {
            Vector2Int direction = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;

            Vector2Int newGridPos = gridPosition + direction;

            if (direction != Vector2Int.zero &&
                GridManager.Instance.IsWithinBounds(newGridPos) &&
                newGridPos.x < GridManager.Instance.columns) // Prevent player from moving into enemy side
            {
                gridPosition = newGridPos;
                targetWorldPosition = GridManager.Instance.GetWorldPosition(gridPosition) + positionOffset;
                isMoving = true;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.01f)
            {
                transform.position = targetWorldPosition;
                isMoving = false;
            }
        }
    }

}