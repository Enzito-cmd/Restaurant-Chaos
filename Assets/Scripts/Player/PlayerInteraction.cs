using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerHoldSystem holdSystem;
    private bool isInteractionLocked;
    private readonly List<LockableStation> visibleLockIcons = new List<LockableStation>();
    private readonly List<PlateRest> visiblePlateRests = new List<PlateRest>();
    private readonly List<ClientHighlight> visibleClientHighlights = new List<ClientHighlight>();
    private readonly List<MoneyHighlight> visibleMoneyHighlights = new List<MoneyHighlight>();

    public bool HasTarget { get; private set; }

    private void Awake()
    {
        holdSystem = GetComponentInChildren<PlayerHoldSystem>();
    }

    private void Update()
    {
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
    private void UpdateMoneyHighlights()
    {
        if (interactPoint == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(
            interactPoint.position,
            interactRadius,
            interactableLayer
        );

        List<MoneyHighlight> currentHighlights = new List<MoneyHighlight>();

        foreach (var hit in hitColliders)
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
        if (interactPoint == null) return;

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

        Collider[] hitColliders = Physics.OverlapSphere(
            interactPoint.position,
            interactRadius,
            interactableLayer
        );

        List<ClientHighlight> currentHighlights = new List<ClientHighlight>();

        foreach (var hit in hitColliders)
        {
            ClientBase client = hit.GetComponentInParent<ClientBase>();

            if (client == null) continue;

            if (!client.IsFrontOfQueue) continue;

            if (client.CurrentState is SitState) continue;
            if (client.IsLeaving) continue;

            ClientHighlight highlight = client.GetComponent<ClientHighlight>();

            if (highlight == null) continue;

            currentHighlights.Add(highlight);
            highlight.SetHighlight(true);
        }

        foreach (ClientHighlight previous in visibleClientHighlights)
        {
            if (!currentHighlights.Contains(previous))
            {
                previous.SetHighlight(false);
            }
        }

        visibleClientHighlights.Clear();
        visibleClientHighlights.AddRange(currentHighlights);
    }
    private void UpdateLockIconsInRange()
    {
        if (interactPoint == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(interactPoint.position, interactRadius, interactableLayer);
        List<LockableStation> stillInRange = new List<LockableStation>();

        foreach (var hit in hitColliders)
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
        if (interactPoint == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(interactPoint.position, interactRadius, interactableLayer);
        List<PlateRest> stillInRange = new List<PlateRest>();

        foreach (var hit in hitColliders)
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
        if (interactPoint == null) return;

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
        }
    }

    private IInteractable FindTarget(bool restrictToExtinguisherOnly)
    {
        if (interactPoint == null)
        {
            return null;
        }

        Collider[] hitColliders = Physics.OverlapSphere(interactPoint.position, interactRadius, interactableLayer);
        IInteractable target = null;

        bool onlyDoorAllowed = DayManager.Instance != null && !DayManager.Instance.CanUseStations;

        foreach (Collider hit in hitColliders)
        {
            if (!hit.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                continue;
            }

            if (onlyDoorAllowed && !(interactable is RestaurantDoor))
            {
                continue;
            }

            if (restrictToExtinguisherOnly)
            {
                if (interactable is ExtinguisherProp)
                {
                    return interactable;
                }

                PlateRest plateRest = interactable as PlateRest;

                if (plateRest != null && plateRest.CanDepositHeldPlate())
                {
                    return interactable;
                }

                continue;
            }

            if (hit.TryGetComponent<LockableStation>(out LockableStation lockable) && lockable.IsLocked)
            {
                continue;
            }

            if (!interactable.CanInteract())
            {
                continue;
            }

            if (interactable is RestaurantClient || interactable is ClientBase)
            {
                return interactable;
            }

            if (target == null)
            {
                target = interactable;
            }
        }

        return target;
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

    private bool IsHoldingExtinguisher()
    {
        if (holdSystem == null) return false;
        if (!holdSystem.IsHoldingItem) return false;

        return holdSystem.GetHeldItem().GetComponentInChildren<ExtinguisherItem>() != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (interactPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
        }
    }
}