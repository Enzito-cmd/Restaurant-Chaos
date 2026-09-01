using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [Header("Chair")]
    [SerializeField] private Transform sitPoint;

    [Header("Free Indicator")]
    [SerializeField] private GameObject freeIndicator;

    private bool isOccupied = false;

    public bool IsOccupied => isOccupied;
    public Transform SitPoint => sitPoint;
    [SerializeField] private Transform moneySpawnPoint;

    [Header("Order Indicator")]
    [SerializeField] private GameObject orderIndicator;
    [SerializeField] private GameObject cookIndicator;
    public GameObject OrderIndicator => orderIndicator;

    public Transform MoneySpawnPoint => moneySpawnPoint;
    private void Start()
    {
        UpdateFreeIndicator(false);
        HideOrderIndicator();
    }
    public void ShowOrderIndicator()
    {
        if (orderIndicator != null)
        {
            orderIndicator.SetActive(true);
            cookIndicator.SetActive(true);
        }
    }

    public void HideOrderIndicator()
    {
        if (orderIndicator != null)
        {
            orderIndicator.SetActive(false);
            cookIndicator.SetActive(false);
        }
    }
    public void Interact()
    {
        if (isOccupied)
        {
            RestaurantClient sittingClient = GetComponentInChildren<RestaurantClient>();

            if (sittingClient == null)
            {
                sittingClient = FindClientNearChair();
            }

            if (sittingClient != null && sittingClient.CurrentState == RestaurantClient.ClientState.Sitting)
            {
                sittingClient.Interact();
            }

            return;
        }

        RestaurantClient client = FindFollowingClient();
        if (client == null) return;

        client.SitOnChair(this);
    }

    private RestaurantClient FindClientNearChair()
    {
        RestaurantClient[] allClients = FindObjectsByType<RestaurantClient>(FindObjectsSortMode.None);
        RestaurantClient nearest = null;
        float minDistance = 2.0f; 

        foreach (var c in allClients)
        {
            if (c.CurrentState == RestaurantClient.ClientState.Sitting)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = c;
                }
            }
        }

        return nearest;
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

        if (isOccupied)
        {
            UpdateFreeIndicator(false);
        }
    }

    public void UpdateFreeIndicator(bool show)
    {
        if (freeIndicator == null)
            return;

        freeIndicator.SetActive(show && !isOccupied);
    }

    public static void ShowFreeChairs()
    {
        Chair[] chairs =
            FindObjectsByType<Chair>(
                FindObjectsSortMode.None
            );

        foreach (Chair chair in chairs)
        {
            chair.UpdateFreeIndicator(true);
        }
    }

    public static void HideAllIndicators()
    {
        Chair[] chairs =
            FindObjectsByType<Chair>(
                FindObjectsSortMode.None
            );

        foreach (Chair chair in chairs)
        {
            chair.UpdateFreeIndicator(false);
        }
    }
}