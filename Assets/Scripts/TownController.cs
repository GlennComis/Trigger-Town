using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TownController : MonoBehaviour
{
    [Header("Buildings")]
    public List<Transform> buildings;
    public float heightPadding = 0.2f;

    [Header("Selection Arrow")]
    public GameObject arrowPrefab;

    [Header("Arrow Animation Settings")]
    public float bounceHeight = 0.25f;
    public float bounceDuration = 0.4f;
    public Ease bounceEase = Ease.InOutSine;

    private GameObject currentArrowInstance;
    private Tween arrowBounceTween;
    private int currentIndex = 0;

    private void Start()
    {
        if (buildings == null || buildings.Count == 0)
        {
            Debug.LogError("No buildings assigned to TownController.");
            return;
        }

        CreateArrowIndicator(buildings[currentIndex]);
    }

    void Update()
    {
        HandleArrowKeyInput();
        HandleSelectionInput();
        // HandleMouseClickInput(); // still disabled
    }

    private void HandleArrowKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % buildings.Count;
            UpdateArrowPosition();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = (currentIndex - 1 + buildings.Count) % buildings.Count;
            UpdateArrowPosition();
        }
    }
    
    private void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Transform currentBuilding = buildings[currentIndex];
            IEnterable enterable = currentBuilding.GetComponent<IEnterable>();

            if (enterable != null)
            {
                enterable.EnterBuilding();
            }
            else
            {
                Debug.LogWarning($"Selected building '{currentBuilding.name}' does not implement IEnterable.", currentBuilding.gameObject);
            }
        }
    }

    /*
    private void HandleMouseClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null)
            {
                GameObject hitGO = hit.gameObject;
                Debug.Log($"Clicked on: {hitGO.name}", hitGO);

                for (int i = 0; i < buildings.Count; i++)
                {
                    Transform building = buildings[i];

                    if (hitGO.transform == building || hitGO.transform.IsChildOf(building))
                    {
                        Debug.Log($"Matched building: {building.name}", building.gameObject);
                        currentIndex = i;
                        UpdateArrowPosition();
                        return;
                    }
                }

                Debug.LogWarning($"Clicked object '{hitGO.name}' is not in buildings list.", hitGO);
            }
            else
            {
                Debug.Log("No collider hit under mouse.");
            }
        }
    }
    */

    private void CreateArrowIndicator(Transform target)
    {
        if (arrowPrefab == null) return;

        currentArrowInstance = Instantiate(arrowPrefab);
        PositionArrow(target);
        AnimateArrow();
    }

    private void UpdateArrowPosition()
    {
        if (currentArrowInstance == null) return;

        arrowBounceTween?.Kill();
        PositionArrow(buildings[currentIndex]);
        AnimateArrow();
    }

    private void PositionArrow(Transform target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        Vector3 anchorPosition = target.position;

        if (sr != null && sr.sprite != null)
        {
            // Get top edge of sprite bounds in local space
            float localTop = sr.sprite.bounds.max.y;

            // Convert local top position to world position
            Vector3 localTopWorld = target.TransformPoint(new Vector3(0, localTop, 0));

            // Set arrow position above that, with extra padding
            anchorPosition = localTopWorld + new Vector3(0, heightPadding, 0);
        }

        currentArrowInstance.transform.position = anchorPosition;
    }


    private void AnimateArrow()
    {
        arrowBounceTween = currentArrowInstance.transform
            .DOMoveY(currentArrowInstance.transform.position.y + bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(-1, LoopType.Yoyo);
    }
}