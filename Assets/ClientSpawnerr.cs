using System.Collections;
using UnityEngine;

public class ClientSpawner : MonoBehaviour
{
    [Header("Client")]
    [SerializeField]
    private ClientAI[] clientPrefabs;


    [Header("Routes")]
    [SerializeField]
    private ClientRoute[] routes;


    [Header("Spawn Time")]
    [SerializeField]
    private float minSpawnTime = 3f;

    [SerializeField]
    private float maxSpawnTime = 8f;


    [Header("Limits")]
    [SerializeField]
    private int maxClients = 10;


    private int activeClients;


    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }


    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay = Random.Range(
                minSpawnTime,
                maxSpawnTime
            );

            yield return new WaitForSeconds(delay);

            if (activeClients >= maxClients)
                continue;

            SpawnClient();
        }
    }


    private void SpawnClient()
    {
        if (routes.Length == 0)
            return;

        if (clientPrefabs.Length == 0)
            return;

        // Ruta aleatoria
        ClientRoute route =
            routes[
                Random.Range(
                    0,
                    routes.Length
                )
            ];


        if (route == null ||
            route.spawnPoint == null)
        {
            return;
        }


        // Cliente aleatorio
        ClientAI prefab =
            clientPrefabs[
                Random.Range(
                    0,
                    clientPrefabs.Length
                )
            ];


        ClientAI client =
            Instantiate(
                prefab,
                route.spawnPoint.position,
                route.spawnPoint.rotation
            );


        activeClients++;


        client.Initialize(route);


        ClientLifetime lifetime =
            client.gameObject.AddComponent<ClientLifetime>();

        lifetime.Initialize(this);
    }


    public void ClientDestroyed()
    {
        activeClients =
            Mathf.Max(
                0,
                activeClients - 1
            );
    }
}