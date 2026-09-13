using UnityEngine;

public class PlayerHoldSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;

    private GameObject currentHeldObject;

    public GameObject CurrentHeldObject => currentHeldObject;


    public bool IsHoldingItem => currentHeldObject != null;

    /// <summary>
    /// Instantiates a new 3D Prefab and places it at the player's HoldPoint.
    /// Fails if the player is already holding an item.
    /// </summary>
    public bool HoldItem(GameObject itemPrefab)
    {
        if (IsHoldingItem)
        {
            Debug.LogWarning("Hands full");
            return false;
        }

        if (itemPrefab == null || holdPoint == null)
        {
            return false;
        }

        currentHeldObject = Instantiate(
            itemPrefab,
            holdPoint
        );

        currentHeldObject.SetActive(true);

        foreach (Transform child in currentHeldObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.SetActive(true);
        }

        ApplyHoldOffset(currentHeldObject);

        return true;
    }

    /// <summary>
    /// Attaches an existing world GameObject to the player's hands.
    /// Fails if the player is already holding an item.
    /// </summary>
    public bool HoldExistingItem(GameObject existingItem)
    {
        if (IsHoldingItem)
        {
            Debug.LogWarning("Hands full");
            return false;
        }

        currentHeldObject = existingItem;
        currentHeldObject.transform.SetParent(holdPoint);
        ApplyHoldOffset(currentHeldObject);

        if (currentHeldObject.TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        return true;
    }

    /// <summary>
    /// Detaches and releases the item currently held by the player, reenabling its collider.
    /// </summary>  
    public GameObject ReleaseItem()
    {
        if (!IsHoldingItem) return null;

        GameObject releasedObject = currentHeldObject;

        if (releasedObject.TryGetComponent<Collider>(out var col))
        {
            col.enabled = true;
        }

        releasedObject.transform.SetParent(null);
        currentHeldObject = null;

        return releasedObject;
    }

    /// <summary>
    /// Completely destroys the item currently held in the player's hands.
    /// </summary>
    public void ClearHeldItem()
    {
        if (currentHeldObject != null)
        {
            Destroy(currentHeldObject);
            currentHeldObject = null;
        }
    }
    public GameObject GetHeldItem()
    {
        return currentHeldObject;
    }

    private void ApplyHoldOffset(GameObject heldObject)
    {
        HeldItemOffset offset = heldObject.GetComponentInChildren<HeldItemOffset>();

        if (offset != null)
        {
            heldObject.transform.localPosition = offset.positionOffset;
            heldObject.transform.localRotation = Quaternion.Euler(offset.rotationOffset);
        }
        else
        {
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
        }
    }
}