using UnityEngine;

namespace RestaurantChaos.Clients
{
    [CreateAssetMenu(menuName = "Restaurant/Meal", fileName = "Meal_")]
    public class MealDefinition : ScriptableObject
    {
        [Header("Info")]
        public string displayName;
        public ItemType itemType;

        [Header("Visual")]
        public GameObject orderVisualPrefab;
        public Sprite uiIcon;

        [Header("Economy")]
        public int basePrice = 10;
    }
}
