using System.Collections.Generic;
using UnityEngine;

public class MochiStation : MonoBehaviour, IInteractable, IMinigame
{
    [Header("Camera")]
    [SerializeField] private GameObject myTransitionCam;

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
                Debug.Log("Incorrect ingredient");
            }
        }
    }

    private void StartMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EnterMinigame(myTransitionCam, this);
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