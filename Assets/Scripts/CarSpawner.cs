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

    // Evita que haya autos que puedan cruzarse
    private bool carIsDriving = false;

    private void Start()
    {
        StartCoroutine(SpawnCarsCoroutine());
    }

    private IEnumerator SpawnCarsCoroutine()
    {
        while (true)
        {
            // Solo intentamos crear un auto si no hay otro circulando
            if (!carIsDriving)
            {
                SpawnCar();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCar()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        if (endPoints == null || endPoints.Length != spawnPoints.Length)
            return;

        // Elegimos un punto al azar
        int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

        Transform spawnPoint = spawnPoints[spawnIndex];
        Transform endPoint = endPoints[spawnIndex];

        if (spawnPoint == null || endPoint == null)
            return;

        // Elegimos un auto al azar
        GameObject carPrefab =
    carPrefabs[UnityEngine.Random.Range(0, carPrefabs.Length)];

        if (carPrefab == null)
            return;

        // Bloqueamos el tráfico
        carIsDriving = true;

        GameObject carObject = Instantiate(
            carPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CarMovement carMovement =
            carObject.GetComponent<CarMovement>();

        if (carMovement != null)
        {
            carMovement.SetTarget(
                endPoint,
                FreeTraffic
            );
        }
        else
        {
            carIsDriving = false;
            Destroy(carObject);
        }
    }

    private void FreeTraffic()
    {
        carIsDriving = false;
    }
}