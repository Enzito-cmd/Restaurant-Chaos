using System.Collections;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class ClientSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int maxClients = 3;
        [SerializeField] private float spawnInterval = 5f;

        [Header("Spawnable Clients")]
        [SerializeField] private ClientSpawnEntry[] availableEntries;

        [Header("References")]
        [SerializeField] private ClientQueue clientQueue;

        public bool HasFinishedSpawning { get; private set; }

        public void SetAvailableEntries(ClientSpawnEntry[] entries)
        {
            availableEntries = entries;
        }

        public void StartSpawning()
        {
            HasFinishedSpawning = false;
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            if (clientQueue == null)
            {
                HasFinishedSpawning = true;
                yield break;
            }

            int amountToSpawn = Mathf.Min(maxClients, clientQueue.Capacity);

            for (int i = 0; i < amountToSpawn; i++)
            {
                SpawnOne();

                if (i < amountToSpawn - 1)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            HasFinishedSpawning = true;
        }

        private void SpawnOne()
        {
            if (availableEntries == null || availableEntries.Length == 0) return;
            if (spawnPoint == null) return;
            if (!clientQueue.HasSpace) return;

            int randomIndex = Random.Range(0, availableEntries.Length);
            ClientSpawnEntry entry = availableEntries[randomIndex];

            if (entry.clientPrefab == null) return;

            GameObject instance = Instantiate(entry.clientPrefab, spawnPoint.position, spawnPoint.rotation);
            ClientBase client = instance.GetComponent<ClientBase>();

            if (client == null) return;

            client.Initialize(entry.config);
            ClientRegistry.Register(client);
            clientQueue.Enqueue(client);
        }
    }
}
