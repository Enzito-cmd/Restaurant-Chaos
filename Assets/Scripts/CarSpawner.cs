using UnityEngine;
using System;
using System.Collections;

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

    // Cantidad de autos que están circulando
    private int carsDriving = 0;

    // Parejas:
    // 1 - 2
    // 3 - 4
    // 5 - 6
    // 7 - 8
    private int currentPair = 0;

    private readonly int[][] spawnPairs =
    {
        new int[] { 0, 1 }, // Spawn 1 y 2
        new int[] { 2, 3 }, // Spawn 3 y 4
        new int[] { 4, 5 }, // Spawn 5 y 6
        new int[] { 6, 7 }  // Spawn 7 y 8
    };

    private void Start()
    {
        StartCoroutine(SpawnCarsCoroutine());
    }

    private IEnumerator SpawnCarsCoroutine()
    {
        while (true)
        {
            // Esperamos a que terminen los dos autos
            if (carsDriving == 0)
            {
                SpawnCarPair();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCarPair()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length != 8)
            return;

        if (endPoints == null || endPoints.Length != 8)
            return;

        // Obtenemos la pareja actual
        int spawnIndex1 = spawnPairs[currentPair][0];
        int spawnIndex2 = spawnPairs[currentPair][1];

        // Spawneamos los dos
        SpawnCar(spawnIndex1);
        SpawnCar(spawnIndex2);

        // Pasamos a la siguiente pareja
        currentPair++;

        if (currentPair >= spawnPairs.Length)
        {
            currentPair = 0;
        }
    }

    private void SpawnCar(int spawnIndex)
    {
        Transform spawnPoint = spawnPoints[spawnIndex];
        Transform endPoint = endPoints[spawnIndex];

        if (spawnPoint == null || endPoint == null)
            return;

        // Elegimos un auto al azar
        GameObject carPrefab =
            carPrefabs[UnityEngine.Random.Range(0, carPrefabs.Length)];

        if (carPrefab == null)
            return;

        GameObject carObject = Instantiate(
            carPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CarMovement carMovement =
    carObject.GetComponent<CarMovement>();

        if (carMovement != null)
        {
            // Velocidad aleatoria entre 20 y 30
            float randomSpeed = UnityEngine.Random.Range(10f, 30f);

            carMovement.SetSpeed(randomSpeed);

            carsDriving++;

            carMovement.SetTarget(
                endPoint,
                FreeTraffic
            );
        }
    }

    private void FreeTraffic()
    {
        carsDriving--;

        if (carsDriving < 0)
        {
            carsDriving = 0;
        }
    }
}