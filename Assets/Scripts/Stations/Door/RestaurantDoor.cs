using RestaurantChaos.Clients;
using UnityEngine;

public class RestaurantDoor : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 120f;

    [Header("Indicator")]
    [SerializeField] private GameObject closedIndicator;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = doorPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        DayManager dayManager = DayManager.Instance;

        bool shouldBeOpen = false;

        if (dayManager != null)
        {
            shouldBeOpen = dayManager.IsRestaurantOpen;
        }

        Quaternion target = closedRotation;

        if (shouldBeOpen)
        {
            target = openRotation;
        }

        doorPivot.localRotation = Quaternion.RotateTowards(doorPivot.localRotation, target, rotationSpeed * Time.deltaTime);

        if (closedIndicator != null)
        {
            closedIndicator.SetActive(CanInteract());
        }
    }

    public void Interact()
    {
        if (DayManager.Instance == null)
        {
            return;
        }

        DayManager.Instance.OpenRestaurant();
    }

    public bool CanInteract()
    {
        DayManager dayManager = DayManager.Instance;

        if (dayManager == null)
        {
            return false;
        }

        return dayManager.CanOpenRestaurant && !dayManager.IsRestaurantOpen;
    }
}
