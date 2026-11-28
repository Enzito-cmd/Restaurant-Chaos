using UnityEngine;

public class FridgeIngredient : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField] private string displayNameSpanish;
    [SerializeField] private GameObject heldPrefab;
    [SerializeField] private GameObject grabVisual;
    [SerializeField] private ClientHighlight highlight;
    [SerializeField] private LockableStation lockedBy;

    public bool IsLocked => lockedBy != null && lockedBy.IsLocked;
    public string DisplayName => Localization.Pick(displayName, displayNameSpanish);
    public GameObject HeldPrefab => heldPrefab;
    public GameObject GrabVisual => grabVisual;

    public void SetHighlight(bool active)
    {
        highlight.SetHighlight(active);
    }
}
