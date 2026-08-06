using UnityEngine;

public class FridgeUI : MonoBehaviour
{
    [System.Serializable]
    public class IngredientItem
    {
        public string ingredientName;
        public GameObject prefab;
    }

    [Header("UI Container")]
    [SerializeField] private GameObject menuPanel;

    [Header("Ingredient Prefabs")]
    [SerializeField] private IngredientItem chocolate;
    [SerializeField] private IngredientItem strawberry;
    [SerializeField] private IngredientItem peach;
    [SerializeField] private IngredientItem seafood;

    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;

    private void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void OpenFridgeMenu()
    {
        if (playerHoldSystem != null && playerHoldSystem.IsHoldingItem)
        {
            Debug.LogWarning("Hands full");
            return;
        }

        if (playerHoldSystem != null && playerHoldSystem.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.SetMovement(false);
        }

        if (menuPanel != null)
            menuPanel.SetActive(true);

        CursorManager.Instance.ShowCursor();
    }

    public void CloseFridgeMenu()
    {
        if (playerHoldSystem != null && playerHoldSystem.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.SetMovement(true);
        }

        if (menuPanel != null)
            menuPanel.SetActive(false);

        CursorManager.Instance.HideCursor();
    }

    public void SelectChocolate() => GiveItemToPlayer(chocolate.prefab);
    public void SelectStrawberry() => GiveItemToPlayer(strawberry.prefab);
    public void SelectPeach() => GiveItemToPlayer(peach.prefab);
    public void SelectSeafood() => GiveItemToPlayer(seafood.prefab);

    private void GiveItemToPlayer(GameObject itemPrefab)
    {
        if (playerHoldSystem != null && itemPrefab != null)
        {
            bool success = playerHoldSystem.HoldItem(itemPrefab);

            if (success)
            {
                CloseFridgeMenu();
            }
        }
    }
}