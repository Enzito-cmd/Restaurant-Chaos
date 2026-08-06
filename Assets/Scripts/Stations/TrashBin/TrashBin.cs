using UnityEngine;

public class TrashBin : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;

    public void Interact()
    {
        if (playerHoldSystem != null)
        {
            if (playerHoldSystem.IsHoldingItem)
            {
                playerHoldSystem.ClearHeldItem();
            }
            else
            {
                Debug.Log("No item in hand");
            }
        }
    }
}