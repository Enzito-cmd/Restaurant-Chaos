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

            for (int i = 0; i < clientCount; i++)
            {
                while (!clientQueue.HasSpace)
                {
                    yield return null;
                }

                if (!SpawnOne())
                {
                    break;
                }

                if (i < clientCount - 1)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            HasFinishedSpawning = true;
            spawnCoroutine = null;
        }

        private bool SpawnOne()
        {
            if (availableEntries == null || availableEntries.Length == 0) return false;
            if (spawnPoint == null) return false;

            int randomIndex = Random.Range(0, availableEntries.Length);
            ClientSpawnEntry entry = availableEntries[randomIndex];

            if (entry.clientPrefab == null) return false;

            GameObject instance = Instantiate(entry.clientPrefab, spawnPoint.position, spawnPoint.rotation);
            SoundManager.Instance?.PlaySound(SoundType.ClientSpawn);
            ClientBase client = instance.GetComponent<ClientBase>();

            if (client == null) return false;

            client.Initialize(entry.config);
            ClientRegistry.Register(client);
            clientQueue.Enqueue(client);

            return true;
        }
    }
}
