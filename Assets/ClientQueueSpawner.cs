using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientQueueSpawner : MonoBehaviour
{
    [Header("Client")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Queue Positions")]
    [SerializeField] private Transform[] queuePositions;

    [Header("Spawn Settings")]
    [SerializeField] private int maxClients = 3;
    [SerializeField] private float spawnInterval = 5f;

    private List<RestaurantClient> clients = new List<RestaurantClient>();

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

            // No esperamos después del último cliente
            if (i < amountToSpawn - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    private void SpawnClient(int queueIndex)
    {
        GameObject clientObject = Instantiate(
            clientPrefab,
            queuePositions[queueIndex].position,
            queuePositions[queueIndex].rotation
        );

        RestaurantClient client = clientObject.GetComponent<RestaurantClient>();

        if (client != null)
        {
            clients.Add(client);
            client.Setup(this, queueIndex);
        }

        UpdateQueue();
    }

    public bool CanPickClient(RestaurantClient client)
    {
        if (clients.Count == 0)
            return false;

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