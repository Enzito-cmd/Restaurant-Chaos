using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private const int MaxNearby = 64;

    [Header("Detection Settings")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRange = 2.2f;
    [SerializeField, Range(10f, 360f)] private float interactAngle = 180f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerHoldSystem holdSystem;
    private bool isInteractionLocked;
    private readonly Collider[] buffer = new Collider[MaxNearby];
    private readonly List<Collider> nearby = new List<Collider>();
    private readonly List<LockableStation> visibleLockIcons = new List<LockableStation>();
    private readonly List<PlateRest> visiblePlateRests = new List<PlateRest>();
    private readonly List<ClientHighlight> visibleClientHighlights = new List<ClientHighlight>();
    private readonly List<MoneyHighlight> visibleMoneyHighlights = new List<MoneyHighlight>();

    public bool HasTarget { get; private set; }

    private Vector3 Origin
    {
        get
        {
            Vector3 origin = transform.position;

            if (interactPoint != null)
            {
                origin.y = interactPoint.position.y;
            }

            return origin;
        }
    }

    private void Awake()
    {
        holdSystem = GetComponentInChildren<PlayerHoldSystem>();
    }

    private void Update()
    {
        RefreshNearby();

        if (!isInteractionLocked && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        UpdateHasTarget();

        UpdateLockIconsInRange();
        UpdatePlateRestsInRange();
        UpdateClientHighlights();
        UpdateMoneyHighlights();
    }

    private void RefreshNearby()
    {
        nearby.Clear();

        Vector3 origin = Origin;
        Vector3 forward = transform.forward;
        float halfAngle = interactAngle * 0.5f;
        int count = Physics.OverlapSphereNonAlloc(origin, interactRange, buffer, interactableLayer);

        for (int i = 0; i < count; i++)
        {
            Vector3 toCollider = buffer[i].bounds.ClosestPoint(origin) - origin;

            toCollider.y = 0f;

            if (toCollider.sqrMagnitude > 0.0001f && Vector3.Angle(forward, toCollider) > halfAngle)
            {
                continue;
            }

            nearby.Add(buffer[i]);
        }
    }

    private void UpdateMoneyHighlights()
    {
        List<MoneyHighlight> currentHighlights = new List<MoneyHighlight>();

        foreach (Collider hit in nearby)
        {
            MoneyPickup money = hit.GetComponentInParent<MoneyPickup>();

            if (money == null) continue;

            MoneyHighlight highlight = money.GetComponent<MoneyHighlight>();

            if (highlight == null) continue;

            currentHighlights.Add(highlight);
            highlight.SetHighlight(true);
        }

        foreach (MoneyHighlight previous in visibleMoneyHighlights)
        {
            if (previous == null)
            {
                continue;
            }

            if (!currentHighlights.Contains(previous))
            {
                previous.SetHighlight(false);
            }
        }

        visibleMoneyHighlights.Clear();
        visibleMoneyHighlights.AddRange(currentHighlights);
    }

    private void UpdateClientHighlights()
    {
        ClientBase followingClient = ClientRegistry.GetFollowingClient();

        if (followingClient != null)
        {
            foreach (ClientHighlight previous in visibleClientHighlights)
            {
                if (previous != null)
                {
                    previous.SetHighlight(false);
                }
            }

            visibleClientHighlights.Clear();
            return;
        }

        List<ClientHighlight> currentHighlights = new List<ClientHighlight>();

        foreach (Collider hit in nearby)
        {
            ClientBase client = hit.GetComponentInParent<ClientBase>();

            if (client == null) continue;

            if (!client.IsFrontOfQueue) continue;

            if (client.CurrentState is SitState) continue;
            if (client.IsEating) continue;
            if (client.IsLeaving) continue;

            ClientHighlight highlight = client.GetComponent<ClientHighlight>();

            if (highlight == null) continue;

            currentHighlights.Add(highlight);
            highlight.SetHighlight(true);
        }

        foreach (ClientHighlight previous in visibleClientHighlights)
        {
            if (previous != null && !currentHighlights.Contains(previous))
            {
                previous.SetHighlight(false);
            }
        }

        visibleClientHighlights.Clear();
        visibleClientHighlights.AddRange(currentHighlights);
    }

    private void UpdateLockIconsInRange()
    {
        List<LockableStation> stillInRange = new List<LockableStation>();

        foreach (Collider hit in nearby)
        {
            if (hit.TryGetComponent<LockableStation>(out var lockable))
            {
                stillInRange.Add(lockable);

                if (!visibleLockIcons.Contains(lockable))
                {
                    lockable.SetIconVisible(true);
                }
            }
        }

        foreach (LockableStation previous in visibleLockIcons)
        {
            if (!stillInRange.Contains(previous))
            {
                previous.SetIconVisible(false);
            }
        }

        visibleLockIcons.Clear();
        visibleLockIcons.AddRange(stillInRange);
    }

    private void UpdatePlateRestsInRange()
    {
        List<PlateRest> stillInRange = new List<PlateRest>();

        foreach (Collider hit in nearby)
        {
            if (hit.TryGetComponent<PlateRest>(out var plateRest))
            {
                stillInRange.Add(plateRest);

                if (!visiblePlateRests.Contains(plateRest))
                {
                    plateRest.SetPlayerInRange(true);
                }
            }
        }

        foreach (PlateRest previous in visiblePlateRests)
        {
            if (!stillInRange.Contains(previous))
            {
                previous.SetPlayerInRange(false);
            }
        }

        visiblePlateRests.Clear();
        visiblePlateRests.AddRange(stillInRange);
    }

    private void TryInteract()
    {
        if (Time.timeScale == 0f) return;

        bool restrictToExtinguisherOnly = IsBeingChased() || IsHoldingExtinguisher();
        IInteractable targetInteractable = FindTarget(restrictToExtinguisherOnly);

        if (targetInteractable != null)
        {
            targetInteractable.Interact();
            return;
        }

        if (restrictToExtinguisherOnly) return;

        ClientBase followingClient = ClientRegistry.GetFollowingClient();

        if (followingClient != null)
        {
            followingClient.Interact();
            return;
        }

        if (IsLockedStationInRange())
        {
            SoundManager.Instance?.PlaySound(SoundType.LockedStation);
        }
    }

    private bool IsLockedStationInRange()
    {
        foreach (Collider hit in nearby)
        {
            if (hit.TryGetComponent<LockableStation>(out LockableStation lockable) && lockable.IsLocked)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidTarget(Collider hit, IInteractable interactable, bool restrictToExtinguisherOnly, bool onlyDoorAllowed)
    {
        if (onlyDoorAllowed && !(interactable is RestaurantDoor))
        {
            return false;
        }

        if (DayManager.Instance != null && !DayManager.Instance.IsInteractionAllowed(interactable))
        {
            return false;
        }

        if (restrictToExtinguisherOnly)
        {
            if (interactable is ExtinguisherProp)
            {
                return interactable.CanInteract();
            }

            PlateRest plateRest = interactable as PlateRest;

            if (plateRest != null && plateRest.CanDepositHeldPlate())
            {
                return true;
            }

            return interactable is TrashBin && interactable.CanInteract() && !IsHoldingExtinguisher();
        }

        if (hit.TryGetComponent<LockableStation>(out LockableStation lockable) && lockable.IsLocked)
        {
            return false;
        }

        return interactable.CanInteract();
    }

    private IInteractable FindTarget(bool restrictToExtinguisherOnly)
    {
        Vector3 origin = Origin;
        bool onlyDoorAllowed = DayManager.Instance != null && !DayManager.Instance.CanUseStations;

        IInteractable clientTarget = null;
        IInteractable otherTarget = null;
        float clientDistance = float.MaxValue;
        float otherDistance = float.MaxValue;

        foreach (Collider hit in nearby)
        {
            if (!hit.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                continue;
            }

            if (!IsValidTarget(hit, interactable, restrictToExtinguisherOnly, onlyDoorAllowed))
            {
                continue;
            }

            float distance = (hit.bounds.ClosestPoint(origin) - origin).sqrMagnitude;

            if (interactable is RestaurantClient || interactable is ClientBase)
            {
                if (distance < clientDistance)
                {
                    clientDistance = distance;
                    clientTarget = interactable;
                }
            }
            else if (distance < otherDistance)
            {
                otherDistance = distance;
                otherTarget = interactable;
            }
        }

        if (clientTarget != null)
        {
            return clientTarget;
        }

        return otherTarget;
    }

    private void UpdateHasTarget()
    {
        if (isInteractionLocked)
        {
            HasTarget = false;
            return;
        }

        bool restrictToExtinguisherOnly = ClientRegistry.AnyChasing || IsHoldingExtinguisher();
        HasTarget = FindTarget(restrictToExtinguisherOnly) != null;
    }

    public void SetLocked(bool locked)
    {
        isInteractionLocked = locked;
    }

    public bool IsBeingChased()
    {
        if (ClientRegistry.AnyChasing) return true;

        RestaurantClient[] clients = FindObjectsByType<RestaurantClient>(FindObjectsSortMode.None);
        foreach (var c in clients)
        {
            if (c.CurrentState == RestaurantClient.ClientState.AngryChasing)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHoldingExtinguisher()
    {
        if (holdSystem == null) return false;
        if (!holdSystem.IsHoldingItem) return false;

        return holdSystem.GetHeldItem().GetComponentInChildren<ExtinguisherItem>() != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 origin = Origin;
        Vector3 left = Quaternion.Euler(0f, -interactAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, interactAngle * 0.5f, 0f) * transform.forward;

        Gizmos.DrawWireSphere(origin, interactRange);
        Gizmos.DrawLine(origin, origin + left * interactRange);
        Gizmos.DrawLine(origin, origin + right * interactRange);
    }
}
