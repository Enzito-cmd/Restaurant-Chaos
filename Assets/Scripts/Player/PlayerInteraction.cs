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