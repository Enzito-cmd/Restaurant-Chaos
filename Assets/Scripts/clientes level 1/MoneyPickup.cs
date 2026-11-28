using UnityEngine;

public class MoneyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int moneyAmount;

    private MoneyHighlight highlight;

    private void Awake()
    {
        highlight = GetComponent<MoneyHighlight>();
    }

    public void SetAmount(int amount)
    {
        moneyAmount = amount;
    }

    public bool CanInteract()
    {
        return PlayerHoldSystem.Instance == null || !PlayerHoldSystem.Instance.IsHoldingItem;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(moneyAmount);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.MoneyPickup);
        }

        if (highlight != null)
        {
            highlight.SetHighlight(false);
        }

        Destroy(gameObject);
    }
}