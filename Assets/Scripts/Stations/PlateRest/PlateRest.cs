using System.Collections.Generic;
using UnityEngine;

public class PlateRest : MonoBehaviour, IInteractable
{
    [Header("Allowed Items")]
    [SerializeField] private List<ItemType> allowedItems;

    [Header("Placement")]
    [SerializeField] private Transform platePoint;

    [Header("Indicator")]
    [SerializeField] private Transform indicatorPoint;
    [SerializeField] private GameObject indicatorPrefab;

    private PlayerHoldSystem playerHoldSystem;
    private GameObject heldPlate;
    private GameObject currentIndicator;
    private bool isPlayerInRange;

    private void Awake()
    {
        playerHoldSystem = FindFirstObjectByType<PlayerHoldSystem>();
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        UpdateIndicator();
    }

    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
        UpdateIndicator();
    }

    public void Interact()
    {
        if (playerHoldSystem == null) return;

        if (heldPlate != null)
        {
            if (playerHoldSystem.IsHoldingItem) return;

            playerHoldSystem.HoldExistingItem(heldPlate);
            heldPlate = null;
            UpdateIndicator();
            return;
        }

        TryDepositHeldPlate();
    }

    public bool TryDepositHeldPlate()
    {
        if (playerHoldSystem == null) return false;
        if (heldPlate != null) return false;
        if (!playerHoldSystem.IsHoldingItem) return false;
        if (!CanDepositHeldItem()) return false;

        GameObject released = playerHoldSystem.ReleaseItem();
        released.transform.SetParent(platePoint, true);
        released.transform.position = platePoint.position;
        released.transform.rotation = platePoint.rotation;

        heldPlate = released;
        UpdateIndicator();
        return true;
    }

    private bool CanDepositHeldItem()
    {
        GameObject held = playerHoldSystem.GetHeldItem();
        return held.TryGetComponent<HoldableItem>(out HoldableItem itemData) && allowedItems.Contains(itemData.itemType);
    }

    private void UpdateIndicator()
    {
        bool canDeposit = heldPlate == null && playerHoldSystem != null && playerHoldSystem.IsHoldingItem && CanDepositHeldItem();
        bool canRetrieve = heldPlate != null && playerHoldSystem != null && !playerHoldSystem.IsHoldingItem;

        bool shouldShow = isPlayerInRange && (canDeposit || canRetrieve);

        if (shouldShow) ShowIndicator();
        else HideIndicator();
    }

    private void ShowIndicator()
    {
        if (currentIndicator != null) return;
        if (indicatorPrefab == null || indicatorPoint == null) return;

        currentIndicator = Instantiate(indicatorPrefab, indicatorPoint.position, indicatorPoint.rotation, indicatorPoint);

        Vector3 prefabScale = indicatorPrefab.transform.localScale;
        Vector3 parentScale = indicatorPoint.lossyScale;
        currentIndicator.transform.localScale = new Vector3(prefabScale.x / parentScale.x, prefabScale.y / parentScale.y, prefabScale.z / parentScale.z);
    }

    private void HideIndicator()
    {
        if (currentIndicator == null) return;

        Destroy(currentIndicator);
        currentIndicator = null;
    }
}
