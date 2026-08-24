using UnityEngine;

public class RestaurantClient : MonoBehaviour, IInteractable
{
    public enum ClientState
    {
        WaitingInQueue,
        FollowingPlayer,
        Sitting,
        Leaving
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Food")]
    [SerializeField] private float foodDeliveryDistance = 2f;

    private Transform player;
    private Transform exitPoint;
    private Chair currentChair;

    private ClientQueueSpawner queueSpawner;
    private Transform targetQueuePosition;

    private ClientState currentState;

    public ClientState CurrentState => currentState;

    private void Awake()
    {
        FindPlayer();
        FindExitPoint();
    }

    public void Setup(ClientQueueSpawner spawner, int queueIndex)
    {
        queueSpawner = spawner;
        currentState = ClientState.WaitingInQueue;

        // Por seguridad, vuelve a buscarlos si todavía son null.
        if (player == null)
        {
            FindPlayer();
        }

        if (exitPoint == null)
        {
            FindExitPoint();
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case ClientState.WaitingInQueue:
                UpdateQueueMovement();
                break;

            case ClientState.FollowingPlayer:
                FollowPlayer();
                break;

            case ClientState.Sitting:
                CheckFoodDelivery();
                break;

            case ClientState.Leaving:
                LeaveRestaurant();
                break;
        }
    }

    // =====================================================
    // AUTOMATIC REFERENCES
    // =====================================================

    private void FindPlayer()
    {
        PlayerController playerController =
            FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            player = playerController.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con PlayerController.");
        }
    }

    private void FindExitPoint()
    {
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");

        if (exit != null)
        {
            exitPoint = exit.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró ningún objeto con el tag Exit.");
        }
    }

    // =====================================================
    // INTERACTION
    // =====================================================

    public void Interact()
    {
        if (currentState != ClientState.WaitingInQueue)
            return;

        if (queueSpawner == null)
            return;

        if (!queueSpawner.CanPickClient(this))
        {
            Debug.Log("Solo podés agarrar al primer cliente de la fila.");
            return;
        }

        StartFollowingPlayer();
    }

    private void StartFollowingPlayer()
    {
        currentState = ClientState.FollowingPlayer;

        queueSpawner.RemoveClientFromQueue(this);

        Debug.Log("Cliente agarrado. Ahora te sigue.");
    }

    // =====================================================
    // QUEUE
    // =====================================================

    public void MoveToQueuePosition(Transform newPosition)
    {
        targetQueuePosition = newPosition;
    }

    private void UpdateQueueMovement()
    {
        if (targetQueuePosition == null)
            return;

        MoveTowards(targetQueuePosition.position);
    }

    // =====================================================
    // FOLLOW PLAYER
    // =====================================================

    private void FollowPlayer()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        Vector3 targetPosition = player.position;

        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );

        if (distance > stopDistance)
        {
            MoveTowards(targetPosition);
        }
    }

    // =====================================================
    // CHAIR
    // =====================================================

    public void SitOnChair(Chair chair)
    {
        if (currentState != ClientState.FollowingPlayer)
            return;

        if (chair == null)
            return;

        if (chair.IsOccupied)
            return;

        currentChair = chair;
        currentChair.SetOccupied(true);

        Transform sitPoint = chair.SitPoint;

        if (sitPoint != null)
        {
            transform.position = sitPoint.position;
            transform.rotation = sitPoint.rotation;
        }
        else
        {
            transform.position = chair.transform.position;
            transform.rotation = chair.transform.rotation;
        }

        currentState = ClientState.Sitting;

        Debug.Log("Cliente sentado.");
    }

    // =====================================================
    // FOOD DELIVERY
    // =====================================================

    private void CheckFoodDelivery()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > foodDeliveryDistance)
            return;

        PlayerHoldSystem holdSystem =
            player.GetComponent<PlayerHoldSystem>();

        if (holdSystem == null)
            return;

        if (!holdSystem.IsHoldingItem)
            return;

        GameObject heldItem = holdSystem.GetHeldItem();

        if (heldItem == null)
            return;

        HoldableItem item = heldItem.GetComponent<HoldableItem>();

        if (item == null)
            return;

        if (item.itemType == ItemType.WokRice)
        {
            DeliverFood(holdSystem);
        }
    }

    private void DeliverFood(PlayerHoldSystem holdSystem)
    {
        Debug.Log("Cliente recibió Wok Rice.");

        holdSystem.ClearHeldItem();

        if (currentChair != null)
        {
            currentChair.SetOccupied(false);
            currentChair = null;
        }

        currentState = ClientState.Leaving;

        Debug.Log("Cliente se va.");
    }

    // =====================================================
    // LEAVING
    // =====================================================

    private void LeaveRestaurant()
    {
        if (exitPoint == null)
        {
            FindExitPoint();

            if (exitPoint == null)
                return;
        }

        MoveTowards(exitPoint.position);

        float distance = Vector3.Distance(
            transform.position,
            exitPoint.position
        );

        if (distance <= 0.5f)
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // MOVEMENT
    // =====================================================

    private void MoveTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= 0.01f)
            return;

        transform.position +=
            direction.normalized *
            moveSpeed *
            Time.deltaTime;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }
}