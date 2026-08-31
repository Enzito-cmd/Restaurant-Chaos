using UnityEngine;
using System.Collections.Generic;

public class ClientLeaveSoundZone : MonoBehaviour
{
    private HashSet<RestaurantClient> clientsPlayed =
        new HashSet<RestaurantClient>();

    private void OnTriggerEnter(Collider other)
    {
        RestaurantClient client =
            other.GetComponentInParent<RestaurantClient>();

        if (client == null)
            return;

        // Solo sonar cuando el cliente se está yendo
        if (client.CurrentState != RestaurantClient.ClientState.Leaving)
            return;

        // Evitar que el mismo cliente lo reproduzca más de una vez
        if (clientsPlayed.Contains(client))
            return;

        clientsPlayed.Add(client);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(
                SoundType.ClientLeave
            );

            Debug.Log("CLIENT LEAVE - SONIDO");
        }
    }
}