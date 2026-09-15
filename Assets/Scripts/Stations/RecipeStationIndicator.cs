using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

public class RecipeStationIndicator : MonoBehaviour
{
    [Header("Produces")]
    [SerializeField] private List<ItemType> producesItemTypes;

    [Header("Indicator")]
    [SerializeField] private Transform indicatorPoint;
    [SerializeField] private GameObject indicatorPrefab;

    private PlayerHoldSystem playerHoldSystem;
    private GameObject currentIndicator;

    private void Awake()
    {
        playerHoldSystem = FindFirstObjectByType<PlayerHoldSystem>();
    }

    private void Update()
    {
        if (IsNeededByAnyOpenOrder())
        {
            ShowIndicator();
        }
        else
        {
            HideIndicator();
        }
    }

    private bool IsNeededByAnyOpenOrder()
    {
        ItemType heldType = GetHeldItemType();

        foreach (OrderTicket ticket in OrderBoard.OpenTickets)
        {
            if (ticket.Meal == null) continue;

            ItemType nextNeeded = ticket.Meal.GetNextRequiredItem(heldType);

            if (producesItemTypes.Contains(nextNeeded))
            {
                return true;
            }
        }

        return false;
    }

    private ItemType GetHeldItemType()
    {
        if (playerHoldSystem == null || !playerHoldSystem.IsHoldingItem) return ItemType.None;

        GameObject held = playerHoldSystem.GetHeldItem();
        return held.TryGetComponent<HoldableItem>(out HoldableItem itemData) ? itemData.itemType : ItemType.None;
    }

    private void ShowIndicator()
    {
        if (currentIndicator != null) return;
        if (indicatorPrefab == null || indicatorPoint == null) return;

        currentIndicator = Instantiate(indicatorPrefab, indicatorPoint.position, indicatorPoint.rotation, indicatorPoint);

        Vector3 prefabScale = indicatorPrefab.transform.localScale;
        Vector3 parentScale = indicatorPoint.lossyScale;
        currentIndicator.transform.localScale = new Vector3(
            prefabScale.x / parentScale.x,
            prefabScale.y / parentScale.y,
            prefabScale.z / parentScale.z);
    }

    private void HideIndicator()
    {
        if (currentIndicator == null) return;

        Destroy(currentIndicator);
        currentIndicator = null;
    }
}
