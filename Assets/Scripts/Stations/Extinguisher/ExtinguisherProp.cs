using UnityEngine;

public class ExtinguisherProp : MonoBehaviour, IInteractable
{
    [Header("Holdable Version")]
    [SerializeField] private GameObject extinguisherHoldPrefab;

    [Header("World Visual")]
    [SerializeField] private GameObject extinguisherPropPrefab; 

    public void Interact()
    {
        PlayerHoldSystem holdSystem = FindFirstObjectByType<PlayerHoldSystem>();
        if (holdSystem == null) return;

        if (!holdSystem.IsHoldingItem)
        {
            if (extinguisherHoldPrefab != null)
            {
                holdSystem.HoldItem(extinguisherHoldPrefab);
                if (extinguisherPropPrefab != null) extinguisherPropPrefab.SetActive(false);
            }
        }
        else
        {
            holdSystem.ClearHeldItem();
            if (extinguisherPropPrefab != null) extinguisherPropPrefab.SetActive(true);
        }
    }

    public void ResetProp()
    {
        if (extinguisherPropPrefab != null) extinguisherPropPrefab.SetActive(true);
    }
}