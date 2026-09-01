using UnityEngine;

public class ExtinguisherFoamCollision : MonoBehaviour
{
    [SerializeField] private float launchForce = 15f;
    [SerializeField] private float upwardForce = 8f;

    private void OnParticleCollision(GameObject other)
    {
        RestaurantClient client = other.GetComponentInParent<RestaurantClient>();

        if (client != null && client.CurrentState == RestaurantClient.ClientState.AngryChasing)
        {
            Vector3 pushDirection = (client.transform.position - transform.position).normalized;
            pushDirection.y = 0;

            client.GetBlownAway(pushDirection * launchForce + Vector3.up * upwardForce);
        }
    }
}