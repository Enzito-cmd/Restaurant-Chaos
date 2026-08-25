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

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Order Visual")]
    [SerializeField] private Transform orderVisualPoint;
    [SerializeField] private GameObject thinkingPrefab;
    [SerializeField] private GameObject wokOrderPrefab;

    private GameObject currentOrderVisual;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        FindPlayer();
        FindExitPoint();
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
                CheckFoodDelivery();
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

        // Si está siguiendo al jugador, está caminando
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

        // Si está yendo a su lugar en la fila
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

        // Si está saliendo del restaurante
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
        Chair.ShowFreeChairs();

        Debug.Log("Cliente agarrado. Ahora te sigue.");
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
        // Apagamos todos los indicadores porque ya sentamos al cliente
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

        Debug.Log("Cliente sentado.");
    }
    private IEnumerator OrderSequence()
    {
        ShowOrderVisual(thinkingPrefab);
        yield return new WaitForSeconds(5f);
        ShowOrderVisual(wokOrderPrefab);
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

        // Siempre delante del cliente
        currentOrderVisual.transform.localPosition =
            new Vector3(0f, 0f, 1.5f);

        currentOrderVisual.transform.localRotation =
            Quaternion.identity;
    }
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