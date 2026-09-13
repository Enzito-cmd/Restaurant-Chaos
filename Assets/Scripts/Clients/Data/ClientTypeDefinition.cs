using UnityEngine;

namespace RestaurantChaos.Clients
{
    [CreateAssetMenu(menuName = "Restaurant/Client Type", fileName = "ClientType_")]
    public class ClientTypeDefinition : ScriptableObject
    {
        [Header("Info")]
        public string displayName;

        [Header("Movement")]
        public float moveSpeed = 3f;
        public float chaseSpeedMultiplier = 1.5f;

        [Header("Queue Patience")]
        public float queuePatienceSeconds = 90f;

        [Header("Seated Patience")]
        public float seatedPatienceSeconds = 90f;
        public PatienceCarryOver[] carryOverRules;

        [Header("Orders")]
        public MealDefinition[] possibleMeals;

        [Header("Aggressive Behavior")]
        public float yellCooldown = 2.5f;
        public float yellDistance = 1.5f;
        public float yellDuration = 1.5f;

        public MealDefinition ChooseMeal()
        {
            if (possibleMeals == null || possibleMeals.Length == 0)
            {
                return null;
            }

            int randomIndex = Random.Range(0, possibleMeals.Length);
            return possibleMeals[randomIndex];
        }

        public float GetSeatedStartFill(float queueEndFill)
        {
            float startFill = 1f;

            if (carryOverRules == null) return startFill;

            foreach (var rule in carryOverRules)
            {
                if (queueEndFill < rule.queueThreshold)
                {
                    startFill = Mathf.Min(startFill, rule.seatedStartFill);
                }
            }

            return startFill;
        }
    }
}
