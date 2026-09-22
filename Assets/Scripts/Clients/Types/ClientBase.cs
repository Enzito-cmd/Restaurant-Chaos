using System;
using UnityEngine;
using UnityEngine.AI;

namespace RestaurantChaos.Clients
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(ClientPatience))]
    [RequireComponent(typeof(ClientOrderDisplay))]
    public abstract class ClientBase : MonoBehaviour, IInteractable
    {
        [Header("Movement")]
        [SerializeField] private float stopDistance = 0.5f;

        [Header("Order")]
        [SerializeField] private GameObject thinkingPrefab;
        [SerializeField] private float thinkingDuration = 5f;
        [SerializeField] private float foodDeliveryDistance = 4f;

        [Header("Money")]
        [SerializeField] private GameObject moneyPrefab;

        [Header("Debug")]
        [SerializeField] private ClientTypeDefinition debugConfig;

        private NavMeshAgent agent;
        private Animator animator;
        private ClientPatience patience;
        private ClientOrderDisplay orderDisplay;
        private ClientStateMachine stateMachine;
        private ClientTypeDefinition config;

        private Transform player;
        private Transform exitPoint;
        private ClientChair currentChair;
        private MealDefinition chosenMeal;
        private bool hasBeenRemoved;

        public event Action LeftQueue;
        public event Action Served;
        public event Action Despawned;

        public NavMeshAgent Agent => agent;
        public Animator Animator => animator;
        public ClientPatience Patience => patience;
        public ClientOrderDisplay OrderDisplay => orderDisplay;
        public ClientTypeDefinition Config => config;
        public Transform Player => player;
        public Transform ExitPoint => exitPoint;
        public float StopDistance => stopDistance;
        public float FoodDeliveryDistance => foodDeliveryDistance;
        public GameObject ThinkingPrefab => thinkingPrefab;
        public float ThinkingDuration => thinkingDuration;

        public bool IsFrontOfQueue { get; set; } = true;

        public bool IsFollowingPlayer => stateMachine.Current is FollowPlayerState;
        public bool IsChasing => stateMachine.Current is AngryChaseState;
        public ClientState CurrentState => stateMachine.Current;

        public ClientChair CurrentChair
        {
            get { return currentChair; }
            set { currentChair = value; }
        }

        public MealDefinition ChosenMeal
        {
            get { return chosenMeal; }
            set { chosenMeal = value; }
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            patience = GetComponent<ClientPatience>();
            orderDisplay = GetComponent<ClientOrderDisplay>();
            stateMachine = new ClientStateMachine();

            if (patience != null)
            {
                patience.Expired += HandlePatienceExpired;
            }

            FindPlayer();
            FindExitPoint();
        }

        private void Update()
        {
            stateMachine.Tick();
        }

        private void LateUpdate()
        {
            if (animator == null) return;
            if (stateMachine.Current == null) return;

            animator.SetBool("Sitting", false);
            stateMachine.Current.UpdateAnimation();
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

        private void HandlePatienceExpired()
        {
            OnPatienceExpired();
        }

        protected abstract void OnPatienceExpired();

        public void Initialize(ClientTypeDefinition clientConfig)
        {
            config = clientConfig;
        }

        public void EnterQueue(Transform queuePosition)
        {
            ChangeState(new QueueState(this, queuePosition));
        }

        public void UpdateQueuePosition(Transform newPosition)
        {
            if (stateMachine.Current is QueueState queueState)
            {
                queueState.SetQueuePosition(newPosition);
            }
        }

        public void SitOnChair(ClientChair chair)
        {
            ChangeState(new SitState(this, chair));
        }

        [ContextMenu("Debug: Enter Queue Here")]
        private void DebugEnterQueueAtSelf()
        {
            if (debugConfig == null) return;

            Initialize(debugConfig);
            EnterQueue(transform);
        }

        public void ChangeState(ClientState nextState)
        {
            stateMachine.ChangeState(nextState);
        }

        public void Interact()
        {
            if (stateMachine.Current == null) return;

            stateMachine.Current.OnInteract();
        }

        public void MoveTowards(Vector3 target, float speed)
        {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.speed = speed;
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(target);
        }

        public void NotifyLeftQueue()
        {
            if (LeftQueue != null)
            {
                LeftQueue.Invoke();
            }
        }

        public bool IsPlayerHoldingItem()
        {
            if (player == null) return false;

            PlayerHoldSystem holdSystem = player.GetComponentInChildren<PlayerHoldSystem>();
            return holdSystem != null && holdSystem.IsHoldingItem;
        }

        public void TryDeliverFood()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > foodDeliveryDistance) return;

            PlayerHoldSystem holdSystem = player.GetComponentInChildren<PlayerHoldSystem>();

            if (holdSystem == null || !holdSystem.IsHoldingItem) return;

            GameObject heldItem = holdSystem.GetHeldItem();
            HoldableItem item = heldItem.GetComponentInChildren<HoldableItem>();

            if (item == null) return;
            if (chosenMeal == null) return;
            if (item.itemType != chosenMeal.itemType) return;

            DeliverFood(holdSystem);
        }

        private void DeliverFood(PlayerHoldSystem holdSystem)
        {
            SoundManager.Instance?.PlaySound(SoundType.FoodDelivered);

            holdSystem.ClearHeldItem();
            SpawnMoneyOnTable();

            if (Served != null)
            {
                Served.Invoke();
            }

            ChangeState(new LeaveState(this));
        }

        private void SpawnMoneyOnTable()
        {
            if (moneyPrefab == null) return;
            if (currentChair == null) return;
            if (currentChair.MoneySpawnPoint == null) return;

            GameObject moneyInstance = Instantiate(moneyPrefab, currentChair.MoneySpawnPoint.position, currentChair.MoneySpawnPoint.rotation);
            SoundManager.Instance?.PlaySound(SoundType.MoneySpawn);

            if (chosenMeal == null) return;

            MoneyPickup moneyPickup = moneyInstance.GetComponent<MoneyPickup>();

            if (moneyPickup != null)
            {
                moneyPickup.SetAmount(chosenMeal.basePrice);
            }
        }

        public void GetBlownAway(Vector3 force)
        {
            if (stateMachine.Current is LeaveState) return;
            if (stateMachine.Current is BlownAwayState) return;

            ChangeState(new BlownAwayState(this, force));
        }

        public void RemoveFromLevel()
        {
            if (hasBeenRemoved) return;

            hasBeenRemoved = true;

            if (Despawned != null)
            {
                Despawned.Invoke();
            }

            Destroy(gameObject);
        }
    }
}
