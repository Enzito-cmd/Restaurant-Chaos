using UnityEngine;

public class MoneyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int moneyAmount = 30;

    public void Interact()
    {
        Debug.Log("Agarraste $" + moneyAmount);

        // Sumar dinero
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(moneyAmount);
        }

        // Sonido al recoger dinero
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.MoneyPickup);
        }

        // Destruir el dinero
        Destroy(gameObject);
    }
}