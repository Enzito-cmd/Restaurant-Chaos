using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [Header("Chair")]
    [SerializeField] private Transform sitPoint;

    private bool isOccupied = false;

    public bool IsOccupied => isOccupied;
    public Transform SitPoint => sitPoint;

    public void Interact()
    {
        if (isOccupied)
        {
            Debug.Log("Esta silla está ocupada.");
            return;
        }

        RestaurantClient client = FindFollowingClient();

        if (client == null)
        {
            Debug.Log("No estás llevando a ningún cliente.");
            return;
        }

        client.SitOnChair(this);
    }

    private RestaurantClient FindFollowingClient()
    {
        RestaurantClient[] clients =
            FindObjectsByType<RestaurantClient>(
                FindObjectsSortMode.None
            );

        foreach (RestaurantClient client in clients)
        {
            if (client.CurrentState ==
                RestaurantClient.ClientState.FollowingPlayer)
            {
                return client;
            }
        }

        return null;
    }

    public void SetOccupied(bool state)
    {
        isOccupied = state;
    }
}