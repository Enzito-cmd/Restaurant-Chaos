using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientQueueSpawner : MonoBehaviour
{
    [Header("Client")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

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

            if (i < amountToSpawn - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    private void SpawnClient(int queueIndex)
    {
        // Aparece atrás de la fila
        GameObject clientObject = Instantiate(
            clientPrefab,
            spawnPoint.position,
            spawnPoint.rotation

        );
        SoundManager.Instance?.PlaySound(SoundType.ClientSpawn);
        RestaurantClient client = clientObject.GetComponent<RestaurantClient>();

        if (client != null)
        {
            clients.Add(client);

            // El cliente sabe a qué posición debe caminar
            client.Setup(this, queueIndex);

            // Camina desde SpawnPoint hasta su lugar en la fila
            client.MoveToQueuePosition(queuePositions[queueIndex]);
        }
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