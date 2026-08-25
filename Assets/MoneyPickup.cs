using UnityEngine;

public class MoneyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int moneyAmount = 30;

    public void Interact()
    {
        Debug.Log("Agarraste $" + moneyAmount);

        // Acá sumamos el dinero
        MoneyManager.Instance.AddMoney(moneyAmount);

        // Destruir el objeto de dinero
        Destroy(gameObject);
    }
}