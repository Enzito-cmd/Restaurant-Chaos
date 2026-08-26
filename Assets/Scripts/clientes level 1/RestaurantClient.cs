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
    [SerializeField] private float foodDeliveryDistance = 4f;

    private Transform player;
    private Transform exitPoint;
    private Chair currentChair;
    private ClientQueueSpawner queueSpawner;
    private Transform targetQueuePosition;
    private ClientState currentState;
    public ClientState CurrentState => currentState;

    [Header("NavMesh")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Order Visual")]
    [SerializeField] private Transform orderVisualPoint;
    [SerializeField] private GameObject thinkingPrefab;
    [SerializeField] private GameObject wokOrderPrefab;

    private GameObject currentOrderVisual;
    private bool playerInFoodRange = false;

    [Header("Money")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private int wokRiceReward = 30;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        FindPlayer();
        FindExitPoint();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            playerInFoodRange = true;
            Debug.Log("Jugador dentro del rango del cliente");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            playerInFoodRange = false;
            Debug.Log("Jugador salió del rango del cliente");
        }
    }
    public void Setup(ClientQueueSpawner spawner, int queueIndex)
    {
        queueSpawner = spawner;
        currentState = ClientState.WaitingInQueue;

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

                if (playerInFoodRange &&
                    Input.GetKeyDown(KeyCode.E))
                {
                    TryDeliverFood();
                }

                break;

            case ClientState.Leaving:
                LeaveRestaurant();
                break;
        }
    }
    private void LateUpdate()
    {
        UpdateAnimation();
    }
    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        if (currentState == ClientState.Sitting)
        {
            animator.SetBool("Sitting", true);
            animator.SetFloat("Speed", 0f);
            return;
        }

        animator.SetBool("Sitting", false);

        bool isWalking = false;

        if (currentState == ClientState.FollowingPlayer)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    player.position
                );

                isWalking = distance > stopDistance;
            }
        }
        if (currentState == ClientState.WaitingInQueue)
        {
            if (targetQueuePosition != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    targetQueuePosition.position
                );

                isWalking = distance > stopDistance;
            }
        }
        if (currentState == ClientState.Leaving)
        {
            if (exitPoint != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    exitPoint.position
                );

                isWalking = distance > 0.5f;
            }
        }

        animator.SetFloat("Speed", isWalking ? 1f : 0f);
    }
    private void FindPlayer()
    {
        PlayerController playerController =
            FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            player = playerController.transform;
        }
    }

    private void FindExitPoint()
    {
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");

        if (exit != null)
        {
            exitPoint = exit.transform;
        }
    }
    public void Interact()
    {
        // Agarrar cliente de la fila
        if (currentState == ClientState.WaitingInQueue)
        {
            if (queueSpawner == null)
                return;
            StartFollowingPlayer();
            return;
        }

        // Entregar comida al cliente sentado
        if (currentState == ClientState.Sitting)
        {
            TryDeliverFood();
        }
    }
    private void StartFollowingPlayer()
    {
        currentState = ClientState.FollowingPlayer;
        queueSpawner.RemoveClientFromQueue(this);
        Chair.ShowFreeChairs();
    }
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
    public void SitOnChair(Chair chair)
    {
        if (currentState != ClientState.FollowingPlayer)
            return;

        if (chair == null || chair.IsOccupied)
            return;

        currentChair = chair;
        currentChair.SetOccupied(true);
        Chair.HideAllIndicators();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        Transform sitPoint = chair.SitPoint;

        if (sitPoint != null)
        {
            transform.position = sitPoint.position;
            transform.rotation = sitPoint.rotation;
        }

        currentState = ClientState.Sitting;
        StartCoroutine(OrderSequence());
    }
    private IEnumerator OrderSequence()
    {
        ShowOrderVisual(thinkingPrefab);
        yield return new WaitForSeconds(5f);
        ShowOrderVisual(wokOrderPrefab);
        if (currentChair != null)
        {
            currentChair.ShowOrderIndicator();
        }
    }
    private void ShowOrderVisual(GameObject prefab)
    {
        if (currentOrderVisual != null)
        {
            Destroy(currentOrderVisual);
        }

        if (prefab == null || orderVisualPoint == null)
            return;

        currentOrderVisual = Instantiate(
            prefab,
            orderVisualPoint.position,
            orderVisualPoint.rotation,
            orderVisualPoint
        );
        currentOrderVisual.transform.localPosition =
            new Vector3(0f, 0f, 1.5f);

        currentOrderVisual.transform.localRotation =
            Quaternion.identity;
    }
    private void TryDeliverFood()
    {
        if (!playerInFoodRange)
        {
            Debug.Log("El jugador no está dentro del rango del cliente");
            return;
        }

        PlayerHoldSystem holdSystem =
            player.GetComponentInChildren<PlayerHoldSystem>();

        if (holdSystem == null)
        {
            Debug.Log("No se encontró PlayerHoldSystem");
            return;
        }

        if (!holdSystem.IsHoldingItem)
        {
            Debug.Log("No tenés comida en la mano");
            return;
        }

        GameObject heldItem = holdSystem.GetHeldItem();

        if (heldItem == null)
        {
            Debug.Log("No hay objeto sostenido");
            return;
        }

        HoldableItem item =
            heldItem.GetComponentInChildren<HoldableItem>();

        if (item == null)
        {
            Debug.Log("El objeto no tiene HoldableItem");
            return;
        }

        if (item.itemType == ItemType.WokRice)
        {
            Debug.Log("Entregando comida");
            DeliverFood(holdSystem);
        }
        else
        {
            Debug.Log("La comida no es WokRice");
        }
    }

    private void DeliverFood(PlayerHoldSystem holdSystem)
    {
        holdSystem.ClearHeldItem();
        if (currentOrderVisual != null)
        {
            Destroy(currentOrderVisual);
            currentOrderVisual = null;
        }
        SpawnMoneyOnTable();
        if (currentChair != null)
        {
            currentChair.SetOccupied(false);
            currentChair.HideOrderIndicator();
            currentChair = null;
        }
        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            5f,
            NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.enabled = true;
            agent.isStopped = false;
            agent.ResetPath();

            currentState = ClientState.Leaving;
        }
    }
    private void SpawnMoneyOnTable()
    {
        if (moneyPrefab == null)
            return;

        if (currentChair == null)
            return;

        Transform spawnPoint = currentChair.MoneySpawnPoint;

        if (spawnPoint == null)
        {
            Debug.LogWarning("La silla no tiene MoneySpawnPoint.");
            return;
        }

        Instantiate(
            moneyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Dinero creado en la mesa: " + wokRiceReward);
    }
    private void LeaveRestaurant()
    {
        if (exitPoint == null)
        {
            FindExitPoint();

            if (exitPoint == null)
                return;
        }

        MoveTowards(exitPoint.position);
    }
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