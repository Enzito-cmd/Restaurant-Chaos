using UnityEngine;

public class LockableStation : MonoBehaviour
{
    [Header("Lock State")]
    [SerializeField] private bool isLocked = true;

    [Header("Visual")]
    [SerializeField] private GameObject lockIcon;

    private bool isPlayerInRange;

    public bool IsLocked => isLocked;

    private void Start()
    {
        UpdateLockIcon();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateLockIcon();
    }

    public void SetIconVisible(bool visible)
    {
        isPlayerInRange = visible;
        UpdateLockIcon();
    }

    private void UpdateLockIcon()
    {
        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked && isPlayerInRange);
        }
    }
}
