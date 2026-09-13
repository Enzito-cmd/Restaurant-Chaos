using UnityEngine;

public class MoneyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int moneyAmount;

    public void SetAmount(int amount)
    {
        moneyAmount = amount;
    }

    public void Interact()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(moneyAmount);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.MoneyPickup);
        }

        Destroy(gameObject);
    }
}