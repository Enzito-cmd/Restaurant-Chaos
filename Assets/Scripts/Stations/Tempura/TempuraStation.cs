using System.Collections.Generic;
using UnityEngine;

public class TempuraStation : MonoBehaviour, IInteractable, IMinigame
{
    [Header("Camera")]
    [SerializeField] private GameObject transitionCam;

    [Header("Accepted ingredients")]
    [SerializeField]
    private List<ItemType> validIngredients = new List<ItemType>
    {
        ItemType.Seafood 
    };

    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    [SerializeField] private TempuraController tempuraController;

    public void Interact()
    {
        if (MinigameManager.Instance != null && (MinigameManager.Instance.isMinigameActive || MinigameManager.Instance.isTransitioning))
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

    private void StartMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EnterMinigame(transitionCam, this);
        }
    }

    public void SetupMinigame()
    {
        if (tempuraController != null) tempuraController.StartMinigame();
    }

    public void EndMinigame()
    {
        if (tempuraController != null) tempuraController.EndMinigame();
    }
}