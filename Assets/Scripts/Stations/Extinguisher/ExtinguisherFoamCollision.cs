using RestaurantChaos.Clients;
using UnityEngine;

public class ExtinguisherFoamCollision : MonoBehaviour
{
    [SerializeField] private float launchForce = 15f;
    [SerializeField] private float upwardForce = 8f;

    private void OnParticleCollision(GameObject other)
    {
        RestaurantClient oldClient = other.GetComponentInParent<RestaurantClient>();

        if (oldClient != null && oldClient.CurrentState == RestaurantClient.ClientState.AngryChasing)
        {
            Vector3 pushDirection = (oldClient.transform.position - transform.position).normalized;
            pushDirection.y = 0;

            oldClient.GetBlownAway(pushDirection * launchForce + Vector3.up * upwardForce);
            return;
        }

        ClientBase newClient = other.GetComponentInParent<ClientBase>();

        if (newClient != null && newClient.IsChasing)
        {
            Vector3 pushDirection = (newClient.transform.position - transform.position).normalized;
            pushDirection.y = 0;

            newClient.GetBlownAway(pushDirection * launchForce + Vector3.up * upwardForce);
        }
    }
}