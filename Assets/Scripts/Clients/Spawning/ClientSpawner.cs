using System.Collections;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class ClientSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;

        [Header("References")]
        [SerializeField] private ClientQueue clientQueue;

        private ClientSpawnEntry[] availableEntries;
        private int clientCount;
        private float spawnInterval;
        private Coroutine spawnCoroutine;

        public bool HasFinishedSpawning { get; private set; }

        public void PrepareDay(DayDefinition day)
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            availableEntries = day.clientEntries;
            clientCount = day.clientCount;
            spawnInterval = day.spawnInterval;

            HasFinishedSpawning = false;
        }

        public void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                return;
            }

            HasFinishedSpawning = false;
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            if (clientQueue == null)
            {
                HasFinishedSpawning = true;
                spawnCoroutine = null;
                yield break;
            }

            int amountToSpawn = Mathf.Min(clientCount, clientQueue.Capacity);

            for (int i = 0; i < amountToSpawn; i++)
            {
                SpawnOne();

                if (i < amountToSpawn - 1)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            HasFinishedSpawning = true;
            spawnCoroutine = null;
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
            SoundManager.Instance?.PlaySound(SoundType.ClientSpawn);
            ClientBase client = instance.GetComponent<ClientBase>();

            if (client == null) return;

            client.Initialize(entry.config);
            ClientRegistry.Register(client);
            clientQueue.Enqueue(client);
        }
    }
}
