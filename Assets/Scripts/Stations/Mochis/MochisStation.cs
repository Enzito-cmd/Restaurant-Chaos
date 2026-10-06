using System.Collections.Generic;
using UnityEngine;

public class MochiStation : MonoBehaviour, IInteractable, IMinigame
{
    [Header("Camera")]
    [SerializeField] private GameObject transitionCam;

    [Header("Accepted ingredients")]
    [SerializeField]
    private List<ItemType> validIngredients = new List<ItemType>
    {
        ItemType.Chocolate,
        ItemType.Peach,
        ItemType.Strawberry
    };

    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    public void Interact()
    {
        if (MinigameManager.Instance != null && MinigameManager.Instance.isMinigameActive)
        {
            return;
        }
        if (playerHoldSystem == null || !playerHoldSystem.IsHoldingItem)
        {
            Debug.Log("Empty hands");
            return;
        }

        GameObject heldObj = playerHoldSystem.GetHeldItem();

        if (heldObj.TryGetComponent<HoldableItem>(out HoldableItem itemData))
        {
            if (validIngredients.Contains(itemData.itemType))
            {
                StartMinigame();
            }
            else
            {
                Debug.Log("Wrong ingredient");
            }
        }
    }

    public bool CanInteract()
    {
        if (MinigameManager.Instance != null && MinigameManager.Instance.IsBusy) return false;
        if (playerHoldSystem == null || !playerHoldSystem.IsHoldingItem) return false;

        GameObject heldObj = playerHoldSystem.GetHeldItem();

        if (!heldObj.TryGetComponent<HoldableItem>(out HoldableItem itemData)) return false;

        return validIngredients.Contains(itemData.itemType);
    }

    private void StartMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EnterMinigame(transitionCam, this);
        }
    }

    public void SetupMinigame()
    {
        Debug.Log("Entering mochis minigame");
    }

    public void EndMinigame()
    {
        Debug.Log("Quitting mochis minigame");
    }
}