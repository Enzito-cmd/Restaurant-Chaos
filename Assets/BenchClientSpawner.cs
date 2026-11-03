using UnityEngine;

public class BenchClientSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] clientPrefabs;
    [SerializeField] private Transform[] seatPoints;

    private void Start()
    {
        SpawnClientsOnBenches();
    }

    private void SpawnClientsOnBenches()
    {
        foreach (Transform seat in seatPoints)
        {
            if (seat == null)
                continue;

            GameObject prefab =
                clientPrefabs[Random.Range(0, clientPrefabs.Length)];

            GameObject client =
                Instantiate(prefab, seat.position, seat.rotation);

            Animator animator = client.GetComponent<Animator>();

            if (animator != null)
            {
                animator.Play("SitNormal");
            }
        }
    }
}