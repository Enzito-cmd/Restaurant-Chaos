
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
    [SerializeField] private float carSpeed = 15f;
    [SerializeField] private int poolSize = 16;

    private List<GameObject> carPool = new List<GameObject>();

    private void Start()
    {
        CreatePool();

        StartCoroutine(SpawnCarsCoroutine());
    }

    private void CreatePool()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
            return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = carPrefabs[
                Random.Range(0, carPrefabs.Length)
            ];

            if (prefab == null)
                continue;

            GameObject car = Instantiate(prefab);

            car.SetActive(false);

            carPool.Add(car);
        }
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
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        if (endPoints == null || endPoints.Length == 0)
            return;

        int spawnIndex = Random.Range(0, spawnPoints.Length);

        if (spawnPoints[spawnIndex] == null)
            return;

        if (spawnIndex >= endPoints.Length)
            return;

        if (endPoints[spawnIndex] == null)
            return;
        GameObject car = GetCarFromPool();

        if (car == null)
            return;

        Transform spawnPoint = spawnPoints[spawnIndex];
        Transform endPoint = endPoints[spawnIndex];

        car.transform.position = spawnPoint.position;
        car.transform.rotation = spawnPoint.rotation;

        CarMovement carMovement = car.GetComponent<CarMovement>();

        if (carMovement == null)
        {
            car.SetActive(false);
            return;
        }
        carMovement.SetSpeed(carSpeed);

        carMovement.SetTarget(
            endPoint,
            () => ReturnCarToPool(car)
        );
        car.SetActive(true);
    }

    private GameObject GetCarFromPool()
    {
        for (int i = 0; i < carPool.Count; i++)
        {
            if (!carPool[i].activeInHierarchy)
            {
                return carPool[i];
            }
        }

        return null;
    }

    private void ReturnCarToPool(GameObject car)
    {
        car.SetActive(false);
    }
}
