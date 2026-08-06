using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 1f;
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
        Collider[] hitColliders = Physics.OverlapSphere(interactPoint.position, interactRadius, interactableLayer);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact();
                break;
            }
        }
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