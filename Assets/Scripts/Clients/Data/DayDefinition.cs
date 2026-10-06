using UnityEngine;

namespace RestaurantChaos.Clients
{
    [CreateAssetMenu(menuName = "Restaurant/Day", fileName = "Day_")]
    public class DayDefinition : ScriptableObject
    {
        [Header("Info")]
        public string displayName;

        [Header("Clients")]
        public ClientSpawnEntry[] clientEntries;
        public int clientCount = 5;
        public float spawnInterval = 10f;
    }
}
