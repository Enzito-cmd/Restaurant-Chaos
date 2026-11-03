using UnityEngine;

public class FridgeIngredient : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField] private GameObject heldPrefab;
    [SerializeField] private GameObject grabVisual;
    [SerializeField] private ClientHighlight highlight;
    [SerializeField] private LockableStation lockedBy;

    public bool IsLocked => lockedBy != null && lockedBy.IsLocked;
    public string DisplayName => displayName;
    public GameObject HeldPrefab => heldPrefab;
    public GameObject GrabVisual => grabVisual;

    public void SetHighlight(bool active)
    {
        highlight.SetHighlight(active);
    }
}
