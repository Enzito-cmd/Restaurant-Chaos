using UnityEngine;

public class ExtinguisherProp : MonoBehaviour, IInteractable
{
    [Header("Holdable Version")]
    [SerializeField] private GameObject extinguisherHoldPrefab;

    [Header("World Visual")]
    [SerializeField] private GameObject extinguisherPropPrefab;

    public bool CanInteract()
    {
        PlayerHoldSystem holdSystem = PlayerHoldSystem.Instance;

        if (holdSystem == null) return false;
        if (!holdSystem.IsHoldingItem) return true;

        return holdSystem.GetHeldItem().GetComponentInChildren<ExtinguisherItem>() != null;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        PlayerHoldSystem holdSystem = PlayerHoldSystem.Instance;

        if (!holdSystem.IsHoldingItem)
        {
            if (extinguisherHoldPrefab != null)
            {
                holdSystem.HoldItem(extinguisherHoldPrefab);
                SoundManager.Instance?.PlaySound(SoundType.ExtinguisherEquip);
                if (extinguisherPropPrefab != null) extinguisherPropPrefab.SetActive(false);
            }
        }
        else
        {
            holdSystem.ClearHeldItem();
            SoundManager.Instance?.PlaySound(SoundType.ExtinguisherEquip);
            if (extinguisherPropPrefab != null) extinguisherPropPrefab.SetActive(true);
        }
    }

    public void ResetProp()
    {
        if (extinguisherPropPrefab != null) extinguisherPropPrefab.SetActive(true);
    }
}
