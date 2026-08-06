using UnityEngine;

public class Fridge : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    [SerializeField] private FridgeUI fridgeUI;

    public void Interact()
    {
        if (fridgeUI != null)
        {
            fridgeUI.OpenFridgeMenu();
        }
    }
}