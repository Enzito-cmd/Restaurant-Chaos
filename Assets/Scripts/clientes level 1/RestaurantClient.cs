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
        Leaving,
        AngryChasing 
    }

    [System.Serializable]
    public struct MealOption
    {
        public ItemType foodType;
        public GameObject visualPrefab;
        public int rewardAmount;
    }


    [Header("Customer Settings")]
    public bool isAngryCustomer = false;

    [Header("Angry Customer Settings")]
    [SerializeField] private float yellDuration = 1.5f;      
    [SerializeField] private float yellDistance = 1.5f;
    [SerializeField] private float yellCooldown = 2.5f;

    private float lastYellTime = -99f;

    [Header("Order Configuration")]
    public MealOption[] possibleMeals;
    private MealOption chosenMeal;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Food Range")]
    [SerializeField] private float foodDeliveryDistance = 4f;

    [Header("NavMesh")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Order Visual")]
    [SerializeField] private Transform orderVisualPoint;
    [SerializeField] private GameObject thinkingPrefab;

    [Header("Money")]
    [SerializeField] private GameObject moneyPrefab;

    private Transform player;
    private Transform exitPoint;
    private Chair currentChair;
    private ClientQueueSpawner queueSpawner;
    private Transform targetQueuePosition;
    private ClientState currentState;
    public ClientState CurrentState => currentState;

    private GameObject currentOrderVisual;
    private bool playerInFoodRange = false;
    private ClientHappiness clientHappiness;
    private bool wasServed = false;
    private bool hasBeenRemoved = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        clientHappiness = GetComponent<ClientHappiness>();
        FindPlayer();
        FindExitPoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            playerInFoodRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            playerInFoodRange = false;
        }
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            playerInFoodRange = true;
        }

        // Si toca el Trigger de la salida mientras se va
        if (currentState == ClientState.Leaving && other.CompareTag("Exit"))
        {
            RemoveClientFromLevel();
        }
    }

    public void Setup(ClientQueueSpawner spawner, int queueIndex)
    {
        queueSpawner = spawner;
        currentState = ClientState.WaitingInQueue;

        if (player == null) FindPlayer();
        if (exitPoint == null) FindExitPoint();
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
                break;

            case ClientState.Leaving:
                LeaveRestaurant();
                break;

            case ClientState.AngryChasing:
                ChasePlayerAngry();
                break;
        }
    }

    private void LateUpdate()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (currentState == ClientState.Sitting)
        {
            animator.SetBool("Sitting", true);
            animator.SetFloat("Speed", 0f);
            return;
        }

        animator.SetBool("Sitting", false);

        if (currentState == ClientState.AngryChasing)
        {
            animator.SetFloat("Speed", 2f); 
            return;
        }

        bool isWalking = false;

        if (currentState == ClientState.FollowingPlayer && player != null)
        {
            isWalking = Vector3.Distance(transform.position, player.position) > stopDistance;
        }
        else if (currentState == ClientState.WaitingInQueue && targetQueuePosition != null)
        {
            isWalking = Vector3.Distance(transform.position, targetQueuePosition.position) > stopDistance;
        }
        else if (currentState == ClientState.Leaving && exitPoint != null)
        {
            isWalking = Vector3.Distance(transform.position, exitPoint.position) > 0.5f;
        }

        animator.SetFloat("Speed", isWalking ? 1f : 0f);
    }

    private void FindPlayer()
    {
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
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
        if (currentState == ClientState.WaitingInQueue)
        {
            if (queueSpawner == null) return;

            if (!queueSpawner.CanPickClient(this))
            {
                Debug.Log("Atendé al primero de la fila.");
                return;
            }

            StartFollowingPlayer();
            return;
        }

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
        if (targetQueuePosition == null) return;

        if (Vector3.Distance(transform.position, targetQueuePosition.position) > stopDistance)
        {
            MoveTowards(targetQueuePosition.position, moveSpeed);
        }
        else if (agent != null && agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            MoveTowards(player.position, moveSpeed);
        }
        else if (agent != null)
        {
            agent.ResetPath();
        }
    }

    public void SitOnChair(Chair chair)
    {
        if (currentState != ClientState.FollowingPlayer) return;
        if (chair == null || chair.IsOccupied) return;

        currentChair = chair;
        currentChair.SetOccupied(true);
        Chair.HideAllIndicators();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (chair.SitPoint != null)
        {
            transform.position = chair.SitPoint.position;
            transform.rotation = chair.SitPoint.rotation;
        }

        currentState = ClientState.Sitting;

        if (possibleMeals != null && possibleMeals.Length > 0)
        {
            if (LevelManager.Instance != null && !LevelManager.Instance.isLevel2)
            {
                chosenMeal = possibleMeals[0];
            }
            else
            {
                int randomMealIndex = Random.Range(0, possibleMeals.Length);
                chosenMeal = possibleMeals[randomMealIndex];
            }
        }

        StartCoroutine(OrderSequence());
    }

    private IEnumerator OrderSequence()
    {
        ShowOrderVisual(thinkingPrefab);
        yield return new WaitForSeconds(5f);

        if (chosenMeal.visualPrefab != null)
        {
            ShowOrderVisual(chosenMeal.visualPrefab);
        }

        if (currentChair != null)
        {
            currentChair.ShowOrderIndicator();
        }
    }

    private void ShowOrderVisual(GameObject prefab)
    {
        if (currentOrderVisual != null) Destroy(currentOrderVisual);
        if (prefab == null || orderVisualPoint == null) return;

        currentOrderVisual = Instantiate(prefab, orderVisualPoint.position, orderVisualPoint.rotation, orderVisualPoint);
        currentOrderVisual.transform.localRotation = prefab.transform.rotation;
    }

    private void TryDeliverFood()
    {
        if (player == null || Vector3.Distance(transform.position, player.position) > foodDeliveryDistance)
        {
            return;
        }

        PlayerHoldSystem holdSystem = player.GetComponentInChildren<PlayerHoldSystem>();
        if (holdSystem == null || !holdSystem.IsHoldingItem)
        {
            return;
        }

        GameObject heldItem = holdSystem.GetHeldItem();
        HoldableItem item = heldItem.GetComponentInChildren<HoldableItem>();

        if (item == null)
        {
            return;
        }

        if (item.itemType == chosenMeal.foodType)
        {
            DeliverFood(holdSystem);
        }
    }
    private void DeliverFood(PlayerHoldSystem holdSystem)
    {
        wasServed = true;
        if (LevelManager.Instance != null) LevelManager.Instance.AddServedClient();
        if (clientHappiness != null) clientHappiness.StopTimer();

        holdSystem.ClearHeldItem();

        if (currentOrderVisual != null)
        {
            Destroy(currentOrderVisual);
            currentOrderVisual = null;
        }

        SpawnMoneyOnTable();

        Vector3 standUpPosition = transform.position;
        if (currentChair != null)
        {
            standUpPosition = currentChair.transform.position + (currentChair.transform.forward * 0.8f);
            currentChair.SetOccupied(false);
            currentChair.HideOrderIndicator();
            currentChair = null;
        }

        if (NavMesh.SamplePosition(standUpPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            standUpPosition = hit.position;
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(standUpPosition); 
            agent.isStopped = false;
            agent.ResetPath();
        }

        currentState = ClientState.Leaving;

        if (exitPoint == null) FindExitPoint();
        if (exitPoint != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(exitPoint.position);
        }
    }

    public void GetBlownAway(Vector3 force)
    {
        if (currentState == ClientState.Leaving) return;
        currentState = ClientState.Leaving; 

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);

        StartCoroutine(DestroyAfterDelay(2.5f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveClientFromLevel();
    }
    private void SpawnMoneyOnTable()
    {
        if (moneyPrefab == null || currentChair == null || currentChair.MoneySpawnPoint == null) return;

        GameObject money = Instantiate(moneyPrefab, currentChair.MoneySpawnPoint.position, currentChair.MoneySpawnPoint.rotation);

        Debug.Log("Money left: " + chosenMeal.rewardAmount);
    }
    //---------------------------------
    public void TriggerAngryChase()
    {
        if (currentState == ClientState.Leaving || currentState == ClientState.AngryChasing) return;

        currentState = ClientState.AngryChasing;

        if (queueSpawner != null)
        {
            queueSpawner.RemoveClientFromQueue(this);
        }

        if (currentChair != null)
        {
            currentChair.SetOccupied(false);
            currentChair.HideOrderIndicator();
            currentChair = null;
        }

        if (currentOrderVisual != null) Destroy(currentOrderVisual);

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }

    private void ChasePlayerAngry()
    {
        if (player == null || agent == null || !agent.isOnNavMesh) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= yellDistance && Time.time >= lastYellTime + yellCooldown)
        {
            lastYellTime = Time.time;
            agent.isStopped = true;
            agent.ResetPath();

            if (animator != null)
            {
                animator.SetTrigger("Yell");
            }

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            return;
        }

        if (Time.time < lastYellTime + yellDuration)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        MoveTowards(player.position, moveSpeed * 1.5f);
    }

    // --------------------------------

    private void LeaveRestaurant()
    {
        if (exitPoint == null)
        {
            FindExitPoint();
            if (exitPoint == null) return;
        }


        if (agent != null && agent.isOnNavMesh)
        {
            agent.stoppingDistance = 0f;
            agent.speed = moveSpeed;
            agent.SetDestination(exitPoint.position);
        }

        Vector3 clientPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 exitPos = new Vector3(exitPoint.position.x, 0f, exitPoint.position.z);

        if (Vector3.Distance(clientPos, exitPos) <= 2.5f)
        {
            RemoveClientFromLevel();
        }
    }
   

    private void MoveTowards(Vector3 target, float speed)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        agent.speed = speed;
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(target);
    }

    public void Die()
    {
        RemoveClientFromLevel();
    }

    private void RemoveClientFromLevel()
    {
        if (hasBeenRemoved) return;
        hasBeenRemoved = true;
        Destroy(gameObject);
    }
}