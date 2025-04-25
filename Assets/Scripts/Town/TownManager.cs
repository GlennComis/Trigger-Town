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

    [Header("Hold Navigation")]
    [SerializeField] private float initialHoldDelay = 0.4f;
    [SerializeField] private float repeatRate = 0.15f;

    private GameObject currentArrowInstance;
    private Tween arrowBounceTween;
    private int currentIndex = 0;
    private bool canSelect = true;

    // Hold input tracking
    private bool isHoldingLeft;
    private bool isHoldingRight;
    private float holdTimerLeft;
    private float holdTimerRight;
    private float repeatTimer;

    private void Start()
    {
        if (buildings == null || buildings.Count == 0)
        {
            Debug.LogError("No buildings assigned to TownManager.");
            return;
        }

        var lastKnownIndex = GameManager.Instance.lastKnowBuildingIndex;
        
        if (lastKnownIndex == -1)
        {
            CreateArrowIndicator(buildings[1]);
            currentIndex = 1;
        }
        else
        {
            CreateArrowIndicator(buildings[lastKnownIndex]);
            currentIndex = lastKnownIndex;
        }
    }

    private void Update()
    {
        if (!canSelect) return;

        HandleArrowKeyHold();
        HandleSelectionInput();
    }

    private void HandleArrowKeyHold()
    {
        bool rightHeld = Input.GetKey(KeyCode.RightArrow);
        bool leftHeld = Input.GetKey(KeyCode.LeftArrow);

        // RIGHT
        if (rightHeld)
        {
            if (!isHoldingRight)
            {
                isHoldingRight = true;
                holdTimerRight = initialHoldDelay;
                MoveArrowRight(); // First move
            }
            else
            {
                holdTimerRight -= Time.deltaTime;
                if (holdTimerRight <= 0f)
                {
                    repeatTimer -= Time.deltaTime;
                    if (repeatTimer <= 0f)
                    {
                        MoveArrowRight();
                        repeatTimer = repeatRate;
                    }
                }
            }
        }
        else
        {
            isHoldingRight = false;
            holdTimerRight = 0f;
        }

        // LEFT
        if (leftHeld)
        {
            if (!isHoldingLeft)
            {
                isHoldingLeft = true;
                holdTimerLeft = initialHoldDelay;
                MoveArrowLeft(); // First move
            }
            else
            {
                holdTimerLeft -= Time.deltaTime;
                if (holdTimerLeft <= 0f)
                {
                    repeatTimer -= Time.deltaTime;
                    if (repeatTimer <= 0f)
                    {
                        MoveArrowLeft();
                        repeatTimer = repeatRate;
                    }
                }
            }
        }
        else
        {
            isHoldingLeft = false;
            holdTimerLeft = 0f;
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

    private void MoveArrowRight()
    {
        currentIndex = (currentIndex + 1) % buildings.Count;
        UpdateArrowPosition();
        repeatTimer = repeatRate;
        UpdateLastKnowBuildingIndex(currentIndex);
    }

    private void MoveArrowLeft()
    {
        currentIndex = (currentIndex - 1 + buildings.Count) % buildings.Count;
        UpdateArrowPosition();
        repeatTimer = repeatRate;
        UpdateLastKnowBuildingIndex(currentIndex);
    }

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

    public void UpdateLastKnowBuildingIndex(int index)
    {
        GameManager.Instance.lastKnowBuildingIndex = index;
    }
}
