using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientQueueSpawner : MonoBehaviour
{
    [Header("Client Prefabs")]
    [SerializeField] private GameObject[] clientPrefabs;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Queue Positions")]
    [SerializeField] private Transform[] queuePositions;

    [Header("Spawn Settings")]
    [SerializeField] private int maxClients = 3;
    [SerializeField] private float spawnInterval = 5f;

    private List<RestaurantClient> clients = new List<RestaurantClient>();

    public bool HasFinishedSpawning { get; private set; } = false;

    private void Start()
    {
        StartCoroutine(SpawnClientsCoroutine());
    }

    private IEnumerator SpawnClientsCoroutine()
    {
        int amountToSpawn = Mathf.Min(maxClients, queuePositions.Length);

        for (int i = 0; i < amountToSpawn; i++)
        {
            SpawnClient(i);

            if (i < amountToSpawn - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        HasFinishedSpawning = true;
    }

    private void SpawnClient(int queueIndex)
    {
        if (clientPrefabs == null || clientPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, clientPrefabs.Length);
        GameObject selectedPrefab = clientPrefabs[randomIndex];

        GameObject clientObject = Instantiate(
            selectedPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.ClientSpawn);
        }

        RestaurantClient client = clientObject.GetComponent<RestaurantClient>();

        if (client != null)
        {
            clients.Add(client);
            client.Setup(this, queueIndex);

            client.MoveToQueuePosition(queuePositions[queueIndex]);
        }
    }

    public bool CanPickClient(RestaurantClient client)
    {
        if (clients.Count == 0) return false;
        return clients[0] == client;
    }

    public void RemoveClientFromQueue(RestaurantClient client)
    {
        if (clients.Contains(client))
        {
            clients.Remove(client);
            UpdateQueue();
        }
    }

    private void UpdateQueue()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] != null)
            {
                clients[i].MoveToQueuePosition(queuePositions[i]);
            }
        }
    }
}