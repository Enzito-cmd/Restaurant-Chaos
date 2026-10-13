using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientSpawnerE : MonoBehaviour
{
    [Header("Client Prefabs")]
    [SerializeField] private GameObject[] clientPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("End Points")]
    [SerializeField] private Transform[] endPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float clientSpeed = 2f;

    [Header("Pool")]
    [SerializeField] private int poolSize = 20;

    private List<GameObject> clientPool = new List<GameObject>();

    private void Start()
    {
        CreatePool();

        StartCoroutine(SpawnClientsCoroutine());
    }

    private void CreatePool()
    {
        if (clientPrefabs == null || clientPrefabs.Length == 0)
            return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = clientPrefabs[
                Random.Range(0, clientPrefabs.Length)
            ];

            if (prefab == null)
                continue;

            GameObject client = Instantiate(prefab);
            client.SetActive(false);

            clientPool.Add(client);
        }
    }

    private IEnumerator SpawnClientsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            SpawnClient();
        }
    }

    private void SpawnClient()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        if (endPoints == null || endPoints.Length != spawnPoints.Length)
            return;

        GameObject client = null;

        for (int i = 0; i < clientPool.Count; i++)
        {
            if (!clientPool[i].activeInHierarchy)
            {
                client = clientPool[i];
                break;
            }
        }

        if (client == null)
            return;

        int spawnIndex = Random.Range(0, spawnPoints.Length);

        Transform spawnPoint = spawnPoints[spawnIndex];
        Transform endPoint = endPoints[spawnIndex];

        if (spawnPoint == null || endPoint == null)
            return;

        client.transform.position = spawnPoint.position;
        client.transform.rotation = spawnPoint.rotation;
        ClientMovementE movement =
            client.GetComponent<ClientMovementE>();

        if (movement == null)
            return;

        movement.SetSpeed(clientSpeed);

        movement.SetTarget(endPoint, () =>
        {
            client.SetActive(false);
        });

        client.SetActive(true);
    }
}