using UnityEngine;
using System.Collections;

public class EnvironmentClientSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnRoute
    {
        public Transform spawnPoint;

        [Tooltip("Puntos que recorrerá el cliente en orden")]
        public Transform[] pathPoints;
    }

    [Header("Client Prefabs")]
    [SerializeField] private GameObject[] clientPrefabs;

    [Header("Spawn Routes")]
    [SerializeField] private SpawnRoute[] spawnRoutes;

    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnTime = 2f;
    [SerializeField] private float maxSpawnTime = 5f;

    private void Start()
    {
        StartCoroutine(SpawnClientsCoroutine());
    }

    private IEnumerator SpawnClientsCoroutine()
    {
        while (true)
        {
            SpawnClient();

            float waitTime = Random.Range(
                minSpawnTime,
                maxSpawnTime
            );

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnClient()
    {
        if (clientPrefabs == null || clientPrefabs.Length == 0)
            return;

        if (spawnRoutes == null || spawnRoutes.Length == 0)
            return;

        // Elegir un spawn/ruta al azar
        SpawnRoute selectedRoute =
            spawnRoutes[Random.Range(0, spawnRoutes.Length)];

        if (selectedRoute.spawnPoint == null)
            return;

        if (selectedRoute.pathPoints == null ||
            selectedRoute.pathPoints.Length == 0)
            return;

        // Elegir cliente
        GameObject clientPrefab =
            clientPrefabs[
                Random.Range(0, clientPrefabs.Length)
            ];

        if (clientPrefab == null)
            return;

        // Crear cliente
        GameObject clientObject = Instantiate(
            clientPrefab,
            selectedRoute.spawnPoint.position,
            selectedRoute.spawnPoint.rotation
        );

        // Configurar movimiento
        EnvironmentClientMovement movement =
            clientObject.GetComponent<EnvironmentClientMovement>();

        if (movement == null)
        {
            Debug.LogWarning(
                "El prefab no tiene EnvironmentClientMovement."
            );

            Destroy(clientObject);
            return;
        }

        movement.SetPath(selectedRoute.pathPoints);
    }
}