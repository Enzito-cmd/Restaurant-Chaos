using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RestaurantClient client =
            other.GetComponentInParent<RestaurantClient>();

        if (client != null &&
            client.CurrentState == RestaurantClient.ClientState.Leaving)
        {
            Destroy(client.gameObject);
        }
    }
}