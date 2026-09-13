using UnityEngine;

namespace RestaurantChaos.Clients
{
    [System.Serializable]
    public struct ClientSpawnEntry
    {
        public GameObject clientPrefab;
        public ClientTypeDefinition config;
    }
}
