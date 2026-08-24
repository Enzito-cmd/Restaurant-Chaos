using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    [Header("NavMesh")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Order Visual")]
    [SerializeField] private Transform orderVisualPoint;
    [SerializeField] private GameObject thinkingPrefab;
    [SerializeField] private GameObject wokOrderPrefab;

    private GameObject currentOrderVisual;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

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

        float distance = Vector3.Distance(
            transform.position,
            targetQueuePosition.position
        );

        if (distance > stopDistance)
        {
            MoveTowards(targetQueuePosition.position);
        }
        else if (agent != null && agent.hasPath)
        {
            agent.ResetPath();
        }
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

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > stopDistance)
        {
            MoveTowards(player.position);
        }
        else if (agent != null)
        {
            agent.ResetPath();
        }
    }

    // =====================================================
    // CHAIR
    // =====================================================

    public void SitOnChair(Chair chair)
    {
        if (currentState != ClientState.FollowingPlayer)
            return;

        if (chair == null || chair.IsOccupied)
            return;

        currentChair = chair;
        currentChair.SetOccupied(true);

        // Desactivamos el agente porque el SitPoint
        // está fuera del NavMesh
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Lo colocamos exactamente en el SitPoint
        Transform sitPoint = chair.SitPoint;

        if (sitPoint != null)
        {
            transform.position = sitPoint.position;
            transform.rotation = sitPoint.rotation;
        }

        currentState = ClientState.Sitting;
        StartCoroutine(OrderSequence());

        Debug.Log("Cliente sentado.");
    }
    private IEnumerator OrderSequence()
    {
        // Muestra el pensamiento
        ShowOrderVisual(thinkingPrefab);

        // Espera 2 segundos
        yield return new WaitForSeconds(5f);

        // Muestra el Wok gris
        ShowOrderVisual(wokOrderPrefab);
    }
    private void ShowOrderVisual(GameObject prefab)
    {
        // Borra el visual anterior
        if (currentOrderVisual != null)
        {
            Destroy(currentOrderVisual);
        }

        if (prefab == null || orderVisualPoint == null)
            return;

        // Crea el nuevo visual arriba de la cabeza
        currentOrderVisual = Instantiate(
            prefab,
            orderVisualPoint.position,
            orderVisualPoint.rotation,
            orderVisualPoint
        );

        // Al ser hijo del punto, queda arriba de la cabeza
        currentOrderVisual.transform.localPosition = Vector3.zero;
        currentOrderVisual.transform.localRotation = Quaternion.identity;
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
        if (currentOrderVisual != null)
        {
            Destroy(currentOrderVisual);
            currentOrderVisual = null;
        }
        if (currentChair != null)
        {
            currentChair.SetOccupied(false);
            currentChair = null;
        }

        if (agent != null)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                transform.position,
                out hit,
                3f,
                NavMesh.AllAreas))
            {
                transform.position = hit.position;

                agent.enabled = true;
                agent.isStopped = false;
            }
            else
            {
                Debug.LogWarning("No se encontró NavMesh cerca de la silla.");
            }
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
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistance;

        agent.SetDestination(target);
    }
}