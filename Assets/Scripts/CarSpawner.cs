using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Prefabs")]
    [SerializeField] private GameObject[] carPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("End Points")]
    [SerializeField] private Transform[] endPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;

    private readonly int[] pairedSpawns =
    {
        5, // Spawn 1  Spawn 6
        4, // Spawn 2  Spawn 5
        7, // Spawn 3  Spawn 8
        6, // Spawn 4  Spawn 7
        1, // Spawn 5  Spawn 2
        0, // Spawn 6  Spawn 1
        3, // Spawn 7  Spawn 4
        2  // Spawn 8  Spawn 3
    };

    private HashSet<int> occupiedPairs = new HashSet<int>();

    private void Start()
    {
        StartCoroutine(SpawnCarsCoroutine());
    }

    private IEnumerator SpawnCarsCoroutine()
    {
        while (true)
        {
            SpawnCar();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCar()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            return;
        }

        if (spawnPoints == null || spawnPoints.Length != 8)
        {

            return;
        }

        if (endPoints == null || endPoints.Length != 8)
        {

            return;
        }

        List<int> availableSpawns = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int pair = GetPair(i);

   
            if (!occupiedPairs.Contains(pair))
            {
                availableSpawns.Add(i);
            }
        }


        if (availableSpawns.Count == 0)
            return;


        int randomListIndex =
            Random.Range(0, availableSpawns.Count);

        int spawnIndex =
            availableSpawns[randomListIndex];

        int pairIndex = GetPair(spawnIndex);

        occupiedPairs.Add(pairIndex);

        Transform spawnPoint = spawnPoints[spawnIndex];
        Transform endPoint = endPoints[spawnIndex];

        if (spawnPoint == null || endPoint == null)
        {
            occupiedPairs.Remove(pairIndex);
            return;
        }


        int randomCar =
            Random.Range(0, carPrefabs.Length);

        GameObject carPrefab =
            carPrefabs[randomCar];

        if (carPrefab == null)
        {
            occupiedPairs.Remove(pairIndex);
            return;
        }
        GameObject carObject = Instantiate(
            carPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        CarMovement carMovement =
            carObject.GetComponent<CarMovement>();

        if (carMovement != null)
        {
            carMovement.SetTarget(
                endPoint,
                () => FreePair(pairIndex)
            );
        }
        else
        {

            occupiedPairs.Remove(pairIndex);
            Destroy(carObject);
        }
    }

    private int GetPair(int spawnIndex)
    {
        return pairedSpawns[spawnIndex];
    }

    private void FreePair(int pairIndex)
    {
        occupiedPairs.Remove(pairIndex);
    }
}