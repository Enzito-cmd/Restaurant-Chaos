using System.Collections.Generic;
using UnityEngine;

public class FryerStation : MonoBehaviour, IInteractable, IMinigame
{
    [Header("Camera")]
    [SerializeField] private GameObject transitionCam;

    [Header("Accepted ingredients")]
    [SerializeField]
    private List<ItemType> validIngredients = new List<ItemType>
    {
        ItemType.BreadedTempura 
    };

    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    [SerializeField] private FryerController fryerController;

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
                playerHoldSystem.ClearHeldItem();
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
        if (fryerController != null) fryerController.StartMinigame();
    }

    public void EndMinigame()
    {
        if (fryerController != null) fryerController.EndMinigame();
    }
}