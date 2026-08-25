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

    public Transform MoneySpawnPoint => moneySpawnPoint;
    private void Start()
    {
        UpdateFreeIndicator(false);
    }

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

        // Si está ocupada, nunca mostramos el indicador
        if (isOccupied)
        {
            UpdateFreeIndicator(false);
        }
    }

    public void UpdateFreeIndicator(bool show)
    {
        if (freeIndicator == null)
            return;

        // Solo se muestra si la silla está libre
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