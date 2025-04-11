using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TownManager : SingletonMonoBehaviour<TownManager>
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

    private bool canSelect = true;

    private void Start()
    {
        if (buildings == null || buildings.Count == 0)
        {
            Debug.LogError("No buildings assigned to TownController.");
            return;
        }

        CreateArrowIndicator(buildings[currentIndex]);
    }

    private void Update()
    {
        if (!canSelect) return;
        HandleArrowKeyInput();
        HandleSelectionInput();
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
            float localTop = sr.sprite.bounds.max.y;
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
    
    /// <summary>
    /// The building index might change if we change the order of the List
    /// Check the buildings variable in the inspector to see the exact order
    /// </summary>
    /// <param name="buildingIndex"></param>
    public void SelectSpecificBuilding(int buildingIndex)
    {
        currentIndex = buildingIndex;
        UpdateArrowPosition();
    }

    public void EnableArrowInstanceGameObject()
    {
        currentArrowInstance.SetActive(true);
    }
    
    public void DisableArrowInstanceGameObject()
    {
        currentArrowInstance.SetActive(false);
    }

    public void EnableBuildingSelection()
    {
        canSelect = true;
    }

    public void DisableBuildingSelection()
    {
        canSelect = false;
    }

    public bool ArrowInstanceExists()
    {
        return currentArrowInstance;
    }
}