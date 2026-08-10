using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class ClientBehaviour : MonoBehaviour
{
    private bool isOnTable = false;
    private bool timerStarted = false;
    private ClientSpawner spawner;
    private float currentSpeed;

    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float maxDistance = 5f;
    private Vector3 originPosition;
    public bool IsBroken => isBroken;
    public bool IsAngry => isAngry;

    [SerializeField] private float angryThreshold = 0.3f; // 30%
    private bool isAngry = false;
    [SerializeField] private float stopDistance = 30f;
    private TypeOfFoods foodAsked;
    private int canTakeOrder; // 0 = Aun no llego a la mesa, 1 = Esta pensando en que pedir, 2 = Ya tiene la orden pensada, 3 = La orden fue recibida
    private SpriteRenderer thinkSprite;

    private TablePoint table;
    private int tableNumber;

    [SerializeField] private float distanceMargin = 0.3f;
    private bool isBroken = false;
    [SerializeField] private GameObject dishReplacementPrefab;

    private Animator animator;
    private int animationState; //Para cambiar entre cada animación (solo en cuenta acciones del cliente normal en este comentario). 0 = Idle, 1 = Caminar, 2 = Sentado, 3 = Comiendo

    public static bool clientsBlocked = false;
    private Collider col;
    public bool _isOnTable => isOnTable;
    public int _tableNumber { get { return tableNumber; } set { tableNumber = value; } }
    public int _canTakeOrder => canTakeOrder;
    public TypeOfFoods _foodAsked => foodAsked;
    public TablePoint _table { get { return table; } set { table = value; } }
    private bool isFollowingPlayer = false;
    [SerializeField] private float followStopDistance = 2f;
    public bool externalMovement;
    public enum ClientType
    {
        Normal,
        Shakuza,
        OldWoman
    }

    [SerializeField] private Transform door;
    [SerializeField] private TextMeshProUGUI timerText;

    private bool isEating = false;
    private float currentEatingTime;
    private bool isLeaving = false;
    [SerializeField] private Sprite[] foodSprites;
    [SerializeField] private SpriteRenderer foodSpriteRenderer;
    private GameObject currentModel;

    [SerializeField] private FeedbackController feedbackController;
    public bool isInQueue = true;
    private NavMeshAgent agent;
    private ClientHappiness happiness;
    private ClientData data;
    private bool initialized = false;
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        happiness = GetComponent<ClientHappiness>();
        timerStarted = true;
        thinkSprite = GetComponentInChildren<SpriteRenderer>();

        if (thinkSprite != null)
            thinkSprite.gameObject.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;

        col = GetComponent<Collider>();
        GameObject doorObj = GameObject.FindGameObjectWithTag("door");

        if (doorObj != null)
            door = doorObj.transform;

        ChangeAnimation(0);
        feedbackController = GameManager.instance._feedbackController;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = followSpeed;
            agent.stoppingDistance = 3f;
            agent.autoBraking = false;
            agent.updateRotation = true;
            agent.updateUpAxis = false;
            agent.isStopped = false;
            agent.updatePosition = true;
        }
    }
    void UpdateModel()
    {
        if (currentModel != null)
            Destroy(currentModel);

        currentModel = Instantiate(data.modelPrefab, transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!data.canBecomeAngry || (!isAngry && !isBroken)) return;

        DishReference dish = other.GetComponentInParent<DishReference>();

        animator.SetTrigger("Attack");

        if (dish != null)
        {
            ReplaceDish(dish);
        }
    }

    private IEnumerator ReturnAngryAnimation()
    {
        yield return new WaitForSeconds(0.5f);

        animator.ResetTrigger("Attack");
    }
    public void SetData(ClientData newData)
    {
        if (newData == null)
        {
            Debug.LogError(
                "ERROR: ClientData es NULL para " + gameObject.name
            );

            return;
        }

        data = newData;

        currentSpeed = data.normalSpeed;

        Debug.Log(
            "ClientData asignado correctamente a " +
            gameObject.name +
            " -> " +
            data.name
        );

        if (data.modelPrefab != null)
        {
            UpdateModel();
        }
        else
        {
            Debug.LogWarning(
                "El ClientData " + data.name +
                " no tiene Model Prefab asignado."
            );
        }

        initialized = true;
    }
    public void StartFollowing(Transform target)
    {
        player = target;
        isFollowingPlayer = true;

        ChangeAnimation(1);
    }
    public void StopFollowing()
    {
        isFollowingPlayer = false;
    }
    public float GetQueueSpeed()
    {
        return data.queueSpeed;
    }
    void Awake()
    {
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        animationState = 0;
    }

    void ReplaceDish(DishReference dish)
    {
        if (dish.replacementObject != null)
        {
            dish.replacementObject.SetActive(true);
        }

        Destroy(dish.gameObject);
    }
    public void SetSpawner(ClientSpawner sp)
    {
        spawner = sp;
        foodAsked = default;
        canTakeOrder = 0;
    }
    public void SetInteractable(bool value)
    {
        if (col != null)
            col.enabled = value;
    }

    private void Update()
    {
        if (!initialized || data == null)
            return;

        if (isEating)
        {
            currentEatingTime -= Time.deltaTime;
            if (timerText != null)
            {
                int timeLeft = Mathf.CeilToInt(currentEatingTime);
                timerText.text = timeLeft.ToString();

                if (timeLeft <= 3)
                {
                    timerText.color = Color.red;
                }
            }

            if (currentEatingTime <= 0)
            {
                if (timerText != null)
                    timerText.gameObject.SetActive(false);

                isEating = false;
                GoToDoor();
            }

            return;
        }
        if (isLeaving && door != null)
        {
            Vector3 dir = door.position - transform.position;
            dir.y = 0;

            if (dir.magnitude > 0.2f)
            {
                dir = dir.normalized;
                transform.position += dir * currentSpeed * Time.deltaTime;
                transform.forward = dir;
            }
            else
            {
                DestroyClient();
            }

            return;
        }
        if (!isBroken)
        {
            if (externalMovement && !data.canBecomeAngry)
            {
                return;
            }
            if (!timerStarted || canTakeOrder == 1)
                return;
            if(isFollowingPlayer && data.clientType == ClientType.OldWoman)
            {
                float speed = followSpeed * data.followMultiplier;
                agent.speed = speed;
            }

            if (isFollowingPlayer && player != null && !(data.canBecomeAngry && (isAngry || isBroken)))
            {
                if (!agent.enabled)
                    agent.enabled = true;

                float idealDistance = 3f;
                float tolerance = 0.5f;
                Vector3 toPlayer = player.position - transform.position;
                float dist = toPlayer.magnitude;

                Vector3 target;

                if (dist < idealDistance - tolerance)
                {
                    Vector3 fleeDir = (transform.position - player.position).normalized;
                    Vector3 rawTarget = transform.position + fleeDir * 4f;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(rawTarget, out hit, 3f, NavMesh.AllAreas))
                        target = hit.position;
                    else
                        target = transform.position;

                    ChangeAnimation(1);
                }
                else if (dist > idealDistance + tolerance)
                {
                    target = player.position;

                    ChangeAnimation(1);
                }
                else
                {
                    agent.ResetPath();
                    target = transform.position;

                    ChangeAnimation(0);
                }

                agent.SetDestination(target);

                if (agent.velocity.sqrMagnitude > 0.01f)
                    transform.forward = agent.velocity.normalized;

                return;
            }
            if (data == null)
            {
                Debug.LogError("Data es NULL en: " + gameObject.name + " ID: " + GetInstanceID());
                return;
            }
            happiness.Tick(currentSpeed * data.happinessMultiplier);
            float percent = happiness.Percent;

            if (data.clientType == ClientType.OldWoman)
            {
                float offsetX = Mathf.Sin(Time.time * 25f) * 2f;

                transform.position += new Vector3(offsetX, 0, 0) * Time.deltaTime;
            };

            if (percent <= 0f)
            {
                if (data.canBecomeAngry && !isAngry)
                {
                    BecomeAngryShakuza();
                }
                else if (!data.canBecomeAngry)
                {
                    Conditions.instance.AddFail();
                    DestroyClient();
                }
            }
        }
        if ((isAngry || isBroken) && data.canBecomeAngry && player != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            float idealDistance = 3f;
            float tolerance = 0.5f;
            Vector3 toPlayer = player.position - transform.position;
            float dist = toPlayer.magnitude;

            Vector3 target;

            if (dist < idealDistance - tolerance)
            {
                Vector3 fleeDir = (transform.position - player.position).normalized;
                Vector3 rawTarget = transform.position + fleeDir * 4f;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(rawTarget, out hit, 3f, NavMesh.AllAreas))
                    target = hit.position;
                else
                    target = transform.position;

                ChangeAnimation(1);
            }
            else if (dist > idealDistance + tolerance)
            {
                target = player.position;

                ChangeAnimation(1);
            }
            else
            {
                agent.ResetPath();
                target = transform.position;

                ChangeAnimation(0);
            }
            agent.SetDestination(target);

            if (agent.velocity.sqrMagnitude > 0.01f)
                transform.forward = agent.velocity.normalized;
            return;
        }
    }
    void GoToDoor()
    {
        isLeaving = true;
        ChangeAnimation(1);
        if (isOnTable)
        {
            table.busy = false;
            table._tableOrder._clientOnTable = null;
        }
    }
    void BecomeAngryShakuza()
    {
        externalMovement = true;
        if (isAngry) return;
        ClientBehaviour.clientsBlocked = true;
        isAngry = true;
        FireUI.instance.ShowFire();
        if (spawner != null)
        {
            spawner.RemoveClient(gameObject);
        }
        transform.position += Vector3.right * 2f;
        if (player == null)
        {
            GameObject playerpos = GameObject.FindGameObjectWithTag("Player");
            if (playerpos != null)
                player = playerpos.transform;
        }
        originPosition = transform.position;
        ChangeAnimationYakuza(animationState);
    }
    public void KillShakuza()
    {
        agent.enabled = false;
        if (!data.canBecomeAngry) return;
        ClientBehaviour.clientsBlocked = false;
        DestroyClient();
    }
    public void OnFloor()
    {
        isInQueue = false;
        if (isOnTable)
        {
            return; 
        }
        currentSpeed = data.floorSpeed;

        ChangeAnimation(0);
    }
    public bool CanBeSeated()
    {
        if (data.clientType != ClientType.OldWoman) return true;

        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        return dist <= 1.5f;
    }
    public void OnTable()
    {
        isInQueue = false;
        if (isBroken) return;
        isOnTable = true;

        happiness.HideBar();
        happiness.ResetBar();

        canTakeOrder = 1;

        ChangeAnimation(2);

        StartCoroutine(ThinkOrder());
    }
    void DestroyClient()
    {
        StopAllCoroutines();
        
        if (spawner != null)
        {
            spawner.RemoveClient(gameObject);
        }
        if (thinkSprite != null)
            thinkSprite.gameObject.SetActive(false);
        if (isOnTable == true)
        {
            table.busy = false;
            table._tableOrder._clientOnTable = null;
            
            if (GameManager.instance._miniGameManager._orderManager._clientOrders.ContainsKey(gameObject) == true)
            {
                GameManager.instance._UIcontroller.RemoveFoodFromTasks(gameObject);
                GameManager.instance._miniGameManager._orderManager.RemoveOrder(gameObject);
            }
        }
            Destroy(gameObject);
    }
    private IEnumerator ThinkOrder()
    {
        thinkSprite.gameObject.SetActive(true);

        float multiplier = 1f;
        if (data.clientType == ClientType.OldWoman)
        {
            multiplier = 5f;
        }
        float timeToThink = Random.Range(data.minThinkingTime, data.maxThinkingTime + 1) * multiplier;

        int foodToAsk = Random.Range(0, data.foods.Length);

        yield return new WaitForSeconds(timeToThink);

        foodAsked = data.foods[foodToAsk];

        thinkSprite.gameObject.SetActive(false);

        if (foodAsked != TypeOfFoods.Mochis)
        {
            thinkSprite.sprite = foodSprites[(int)foodAsked];
        }
        else
        {
            thinkSprite.sprite = foodSprites[3];
        }
        thinkSprite.gameObject.SetActive(true);

        if (foodAsked == TypeOfFoods.WorkRice)
        {
            CookUI.instance.ShowCook();
        }
        else if (foodAsked == TypeOfFoods.Takoyaki)
        {
            CookUI.instance.Showtempuu();
        }

        feedbackController.PlayParticle(foodToAsk);

        happiness.ShowBar();

        canTakeOrder = 2;
        if (TutorialManager.instance != null)
            TutorialManager.instance.OnActionTriggered(TutorialManager.TutorialStep.SeatClient);

        GameManager.instance._UIcontroller.SetTaskImage(TakeOrder(), tableNumber);
    }
    public TypeOfFoods TakeOrder ()
    {
        canTakeOrder = 3;
        GameManager.instance._miniGameManager._orderManager.AddOrder(gameObject);
        happiness.ResetBar();
        return foodAsked;
    }
    public void Payment(float defaultValueOfFood)
    {
        GameManager.instance._economiyBehaviour.IncreaseMoneyForClient(defaultValueOfFood,happiness.CurrentTime / happiness.MaxTime);

        player.gameObject.GetComponent<MiniGameManager>()._orderManager.SatisfiedClient(); //Se aumenta el contador de clientes satisfechos
        StartEating();
    }
    void StartEating()
    {
        isEating = true;
        currentEatingTime = data.eatingTime;

        ChangeAnimation(3);
        happiness.HideBar();
        if (timerText != null)
            timerText.gameObject.SetActive(true);
        if (thinkSprite != null)
            thinkSprite.gameObject.SetActive(false);
    }
    public void ChangeAnimation(int animationToChange) //0 = Idle, 1 = Caminar, 2 = Sentado, 3 = Comiendo
    {
        animationState = animationToChange;
        animator.SetInteger("State", animationState);
    }
    public void ChangeAnimationYakuza (int animationToChange)
    {
        animationState = animationToChange;
        animator.SetInteger("State", animationState);
        animator.SetBool("IsAngry", isAngry);
    }
    public void ChangeAnimationYakuza (float distancePlayer)
    {
        animator.SetFloat("Distance", distancePlayer);
    }
    private void OnDestroy()
    {
        if (data != null && data.canBecomeAngry)
        {
            clientsBlocked = false;
        }
    }
}