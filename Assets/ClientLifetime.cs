using UnityEngine;

public class ClientLifetime : MonoBehaviour
{
    private ClientSpawner spawner;

    public void Initialize(
        ClientSpawner owner)
    {
        spawner = owner;
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.ClientDestroyed();
    }
}