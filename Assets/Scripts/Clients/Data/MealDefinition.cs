using System.Collections.Generic;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    [CreateAssetMenu(menuName = "Restaurant/Meal", fileName = "Meal_")]
    public class MealDefinition : ScriptableObject
    {
        [Header("Info")]
        public string displayName;
        public ItemType itemType;

        [Header("Recipe")]
        public List<ItemType> requiredItems;

        [Header("Visual")]
        public GameObject orderVisualPrefab;
        public Sprite uiIcon;

        [Header("Economy")]
        public int basePrice = 10;

        public ItemType GetNextRequiredItem(ItemType heldItemType)
        {
            if (heldItemType == itemType) return ItemType.None;

            for (int i = 0; i < requiredItems.Count; i++)
            {
                if (requiredItems[i] == heldItemType)
                {
                    return i + 1 < requiredItems.Count ? requiredItems[i + 1] : itemType;
                }
            }

            return requiredItems.Count > 0 ? requiredItems[0] : itemType;
        }
    }
}
