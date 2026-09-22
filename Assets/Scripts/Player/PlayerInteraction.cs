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
    private readonly List<LockableStation> visibleLockIcons = new List<LockableStation>();
    private readonly List<PlateRest> visiblePlateRests = new List<PlateRest>();
    private readonly List<ClientHighlight> visibleClientHighlights = new List<ClientHighlight>();

    private void Awake()
    {
        holdSystem = GetComponentInChildren<PlayerHoldSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        UpdateLockIconsInRange();
        UpdatePlateRestsInRange();
        UpdateClientHighlights();
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

        Collider[] hitColliders = Physics.OverlapSphere(interactPoint.position, interactRadius, interactableLayer);

        bool restrictToExtinguisherOnly = IsBeingChased() || IsHoldingExtinguisher();
        IInteractable targetInteractable = null;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                if (restrictToExtinguisherOnly)
                {
                    if (interactable is ExtinguisherProp)
                    {
                        interactable.Interact();
                        return;
                    }

                    if (interactable is PlateRest plateRest && plateRest.TryDepositHeldPlate())
                    {
                        return;
                    }

                    continue;
                }

                if (hit.TryGetComponent<LockableStation>(out var lockable) && lockable.IsLocked)
                {
                    continue;
                }

                if (interactable is RestaurantClient || interactable is ClientBase)
                {
                    targetInteractable = interactable;
                    break;
                }

                if (targetInteractable == null)
                {
                    targetInteractable = interactable;
                }
            }
        }

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