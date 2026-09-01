using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (interactPoint == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(interactPoint.position, interactRadius, interactableLayer);

        bool isChased = IsBeingChased();
        IInteractable targetInteractable = null;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                if (isChased)
                {
                    if (interactable is ExtinguisherProp)
                    {
                        interactable.Interact();
                        return;
                    }
                    continue; 
                }

                if (interactable is RestaurantClient)
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
        }
    }

    public bool IsBeingChased()
    {
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

    private void OnDrawGizmosSelected()
    {
        if (interactPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
        }
    }
}