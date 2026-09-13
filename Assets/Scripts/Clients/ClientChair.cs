using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class ClientChair : MonoBehaviour, IInteractable
    {
        [Header("Seating")]
        [SerializeField] private int tableNumber = 1;
        [SerializeField] private bool isSittable = true;

        [Header("References")]
        [SerializeField] private Transform sitPoint;
        [SerializeField] private Transform moneySpawnPoint;
        [SerializeField] private Transform freeIndicatorPoint;
        [SerializeField] private GameObject freeIndicatorPrefab;

        private bool isOccupied;
        private GameObject currentFreeIndicator;

        public bool IsOccupied => isOccupied;
        public bool IsSittable => isSittable;
        public int TableNumber => tableNumber;
        public Transform SitPoint => sitPoint;
        public Transform MoneySpawnPoint => moneySpawnPoint;

        private void Start()
        {
            UpdateFreeIndicator(false);
        }

        public void Interact()
        {
            if (!isSittable) return;
            if (isOccupied) return;

            ClientBase followingClient = FindFollowingClient();

            if (followingClient == null) return;

            followingClient.SitOnChair(this);
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;

            if (isOccupied)
            {
                UpdateFreeIndicator(false);
            }
        }

        public void SetSittable(bool sittable)
        {
            isSittable = sittable;

            if (!isSittable)
            {
                ClearFreeIndicator();
            }
        }

        public void UpdateFreeIndicator(bool show)
        {
            bool shouldShow = show && !isOccupied && isSittable;

            if (shouldShow)
            {
                ShowFreeIndicator();
            }
            else
            {
                ClearFreeIndicator();
            }
        }

        private void ShowFreeIndicator()
        {
            if (currentFreeIndicator != null) return;
            if (freeIndicatorPrefab == null) return;
            if (freeIndicatorPoint == null) return;

            currentFreeIndicator = Instantiate(freeIndicatorPrefab, freeIndicatorPoint.position, freeIndicatorPoint.rotation, freeIndicatorPoint);

            Vector3 prefabScale = freeIndicatorPrefab.transform.localScale;
            Vector3 parentScale = freeIndicatorPoint.lossyScale;
            currentFreeIndicator.transform.localScale = new Vector3(
                prefabScale.x / parentScale.x,
                prefabScale.y / parentScale.y,
                prefabScale.z / parentScale.z);
        }

        private void ClearFreeIndicator()
        {
            if (currentFreeIndicator == null) return;

            Destroy(currentFreeIndicator);
            currentFreeIndicator = null;
        }

        private ClientBase FindFollowingClient()
        {
            ClientBase[] clients = FindObjectsByType<ClientBase>(FindObjectsSortMode.None);

            foreach (ClientBase candidate in clients)
            {
                if (candidate.IsFollowingPlayer)
                {
                    return candidate;
                }
            }

            return null;
        }

        public static void ShowFreeChairs()
        {
            ClientChair[] chairs = FindObjectsByType<ClientChair>(FindObjectsSortMode.None);

            foreach (ClientChair chair in chairs)
            {
                chair.UpdateFreeIndicator(true);
            }
        }

        public static void HideAllIndicators()
        {
            ClientChair[] chairs = FindObjectsByType<ClientChair>(FindObjectsSortMode.None);

            foreach (ClientChair chair in chairs)
            {
                chair.UpdateFreeIndicator(false);
            }
        }
    }
}
