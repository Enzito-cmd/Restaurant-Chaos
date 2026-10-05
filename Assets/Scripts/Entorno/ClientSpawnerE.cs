using UnityEngine;
using System.Collections;
using System;

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

    private int clientsWalking = 0;

    private int currentPair = 0;

    // Orden inverso al tráfico
    private readonly int[][] spawnPairs =
    {
    new int[] { 6, 7 }, // 7 y 8
    new int[] { 4, 5 }, // 5 y 6
    new int[] { 2, 3 }, // 3 y 4
    new int[] { 0, 1 } // 1 y 2
};

    private void Start()
    {
        StartCoroutine(SpawnClientsCoroutine());
    }

    private IEnumerator SpawnClientsCoroutine()
    {
        while (true)
        {
            if (clientsWalking == 0)
            {
                SpawnClientPair();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnClientPair()
    {
        if (clientPrefabs == null || clientPrefabs.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length != 8)
            return;

        if (endPoints == null || endPoints.Length != 8)
            return;

        int spawnIndex1 = spawnPairs[currentPair][0];
        int spawnIndex2 = spawnPairs[currentPair][1];

        SpawnClient(spawnIndex1);
        SpawnClient(spawnIndex2);

        currentPair++;

        if (currentPair >= spawnPairs.Length)
        {
            currentPair = 0;
        }
    }

    private void SpawnClient(int spawnIndex)
    {
        Transform spawnPoint = spawnPoints[spawnIndex];
        Transform endPoint = endPoints[spawnIndex];

        if (spawnPoint == null || endPoint == null)
            return;

        GameObject clientPrefab =
        clientPrefabs[UnityEngine.Random.Range(0, clientPrefabs.Length)];

        if (clientPrefab == null)
            return;

        GameObject clientObject = Instantiate(
        clientPrefab,
        spawnPoint.position,
        spawnPoint.rotation
        );

        ClientMovementE clientMovement =
        clientObject.GetComponent<ClientMovementE>();

        if (clientMovement != null)
        {
            float randomSpeed =
            UnityEngine.Random.Range(1.5f, 3.5f);

            clientMovement.SetSpeed(randomSpeed);

            clientsWalking++;

            clientMovement.SetTarget(
            endPoint,
            ClientFinished
            );
        }
    }

    private void ClientFinished()
    {
        clientsWalking--;

        if (clientsWalking < 0)
        {
            clientsWalking = 0;
        }
    }
}


