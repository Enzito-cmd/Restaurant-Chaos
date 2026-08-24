using UnityEngine;
using System.Collections.Generic;

public class ClientQueueSpawner : MonoBehaviour
{
    [Header("Client")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Queue Positions")]
    [SerializeField] private Transform[] queuePositions;

    private List<RestaurantClient> clients = new List<RestaurantClient>();

    private void Start()
    {
        SpawnClients();
    }

    private void SpawnClients()
    {
        int amountToSpawn = Mathf.Min(3, queuePositions.Length);

        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject clientObject = Instantiate(
                clientPrefab,
                queuePositions[i].position,
                queuePositions[i].rotation
            );

            RestaurantClient client = clientObject.GetComponent<RestaurantClient>();

            if (client != null)
            {
                clients.Add(client);

                client.Setup(this, i);
            }
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