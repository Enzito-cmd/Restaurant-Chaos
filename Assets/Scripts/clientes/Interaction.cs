using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    [SerializeField] private float distanceRaycast = 2f;
    [SerializeField] private float sphereRadius = 1f; // Solo agregamos esto
    [SerializeField] private InputAction interactAction;
    private GameObject heldObject;
    [SerializeField] private Transform grabPoint;
    private ClientSpawner spawner;
    private GameObject clientHeld;
    private TablePoint[] tables;
    [SerializeField] private GameObject pressFText;
    private DealingController dealingController;
    private ClientBehaviour followingClient;
    public bool canInteract = true;

    [Header ("Advise Message")]
    [SerializeField] private Transform interactionAdvise;
    private Sprite defaultSprite;
    [SerializeField] private Sprite missingFood;

    [Header("Sound Section")]
    [SerializeField] private AudioClip deliveryPlateSound;
    [SerializeField] private AudioClip takeExtinguisherSound;
    [SerializeField] private AudioClip dropExtinguisherSound;
    [SerializeField] private AudioClip defalutTakeObject;
    [SerializeField] private AudioClip grabFoodSound;
    private SoundManager soundManager;

    public GameObject _heldObject { get { return heldObject; } set { heldObject = value; } }
    private void Awake()
    {
        interactAction.Enable();
        spawner = Object.FindFirstObjectByType<ClientSpawner>();
        tables = Object.FindObjectsByType<TablePoint>(FindObjectsSortMode.None);

        dealingController = GetComponent<DealingController>();
    }

    private void Start()
    {
        defaultSprite = interactionAdvise.gameObject.GetComponent<SpriteRenderer>().sprite;

        soundManager = GameManager.instance._soundManager;
    }

    private void Update()
    {
        if (!canInteract) return;
        if (ClientBehaviour.clientsBlocked && followingClient != null)
        {
            ForceDropClient();
        }
        DetectObjects();
        if (interactAction.triggered)//activa input x frame
        {
            if (heldObject == null && clientHeld == null && followingClient == null)
            {
                Grab();
            }
            else
            {
                Drop();
            }
        }
        if (heldObject != null)
        {
            heldObject.transform.position = grabPoint.position;//mueve el objeto agarrado a la posicion del jugador
            //heldObject.transform.rotation = heldRotation;
        }
    }
    public void TableIndicators(bool state)
    {
        foreach (var table in tables)
        {
            table.FreeIndicator(state);
        }
    }
    public void ShowText(bool value)
    {
        pressFText.SetActive(value);
    }

    private bool MySphereCast(out RaycastHit hit)
    {
        Vector3 origin = transform.position - transform.forward * 0.5f;
        return Physics.SphereCast(origin, sphereRadius, transform.forward, out hit, distanceRaycast + 0.5f);
    }
    private void ForceDropClient()
    {
        if (followingClient == null) return;

        followingClient.StopFollowing();

        NavMeshAgent agent = followingClient.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = true; 
        }

        followingClient.OnFloor();

        clientHeld = null;
        followingClient = null;

        TableIndicators(false);
    }

    void Grab()
    {
        if (followingClient != null) return;
        RaycastHit hit;
        if (MySphereCast(out hit))
        {
            FireExtinguisherBehaviour extinguisher = hit.collider.GetComponentInParent<FireExtinguisherBehaviour>();

            if (extinguisher != null)
            {
                heldObject = extinguisher.gameObject;

                heldObject.GetComponent<Collider>().enabled = false;
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                heldObject.transform.SetParent(grabPoint);

                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
                heldObject.transform.localRotation = Quaternion.Euler(0, -90, 0);

                ShowText(true);

                if (heldObject.GetComponent<ItemsBase>())
                {
                    dealingController.SetItemGrabbed(heldObject.GetComponent<ItemsBase>());
                }

                soundManager.ReproduceSound(takeExtinguisherSound);
                return;
            }
            if (ClientBehaviour.clientsBlocked) return;

           
            if (hit.collider.CompareTag("Client"))
            {
                ClientBehaviour clientBehaviour = hit.collider.GetComponent<ClientBehaviour>();
                //ClientBehaviour clientBehaviour = hit.collider.GetComponentInParent<ClientBehaviour>();

                if (clientBehaviour._isOnTable)
                {
                    return;
                }

                if (clientBehaviour == null)
                    return;

                if (clientBehaviour.isInQueue)
                {
                    if (spawner != null && spawner.HasClients())
                    {
                        GameObject first = spawner.GetFirstClient();

                        if (hit.collider.gameObject != first)
                            return;

                        TableIndicators(true);

                        clientHeld = first;

                        ClientBehaviour cb = first.GetComponent<ClientBehaviour>();

                        cb.StartFollowing(transform);

                        followingClient = cb;

                        cb.isInQueue = false;

                        spawner.RemoveFirstClient();

                        return;
                    }
                }
                else
                {
                    TableIndicators(true);

                    clientHeld = clientBehaviour.gameObject;

                    clientBehaviour.StartFollowing(transform);

                    followingClient = clientBehaviour;

                    return;
                }

        }///esto no
            if (hit.collider.CompareTag("Grabbable"))
            {
                heldObject = hit.collider.gameObject;//guarda el objeto agarrado

                heldObject.GetComponent<Collider>().enabled = false;
                if (heldObject.GetComponent<ItemsBase>()) { dealingController.SetItemGrabbed(heldObject.GetComponent<ItemsBase>()); }
                else if (heldObject.GetComponent<FoodBehaviour>()!= null) { soundManager.ReproduceSound(grabFoodSound); }
            }


        }
    }
    public void Drop()
    {
        RaycastHit hit;

        if (followingClient != null)
        {
            if (MySphereCast(out hit))
            {
                if (hit.collider.CompareTag("TablePoint"))
                {
                    TablePoint table = hit.collider.GetComponent<TablePoint>();
                    if (table == null || table.busy) return;

                    //if (!followingClient.CanBeSeated())
                    //{
                    //    return;
                    //}

                    followingClient.StopFollowing();
                    NavMeshAgent agent = followingClient.GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.ResetPath();
                        agent.enabled = false;
                    }

                    followingClient.transform.position = table.transform.position;
                    followingClient.transform.rotation = table.transform.rotation;

                    table._tableOrder._clientOnTable = followingClient;
                    table.busy = true;

                    followingClient.OnTable();
                    followingClient._table = table;
                    followingClient._tableNumber = table._numberOfTable;

                    TableIndicators(false);
                    clientHeld = null;
                    followingClient = null;
                    return;
                }
            }

            if (clientHeld != null)
            {
                ClientBehaviour floorClient = clientHeld.GetComponent<ClientBehaviour>();
                if (floorClient != null)
                {
                    floorClient.StopFollowing();
                    floorClient.OnFloor();
                }
            }
            clientHeld = null;
            followingClient = null;
            TableIndicators(false);
            return;
        }
        ///esto no
        if (heldObject.GetComponent<FoodBehaviour>() != null)
        {
            if (MySphereCast(out hit))
            {
                if (hit.collider.gameObject.GetComponent<TableOrder>() != null)
                {
                    FoodBehaviour foodToDelivery = heldObject.GetComponent<FoodBehaviour>();
                    hit.collider.gameObject.GetComponent<TableOrder>().LeftFood(foodToDelivery, foodToDelivery._valueOfFood);

                    soundManager.ReproduceSound(deliveryPlateSound);
                }
                else if ((hit.collider.CompareTag("MinigameTable") == true) || hit.collider.CompareTag("Oven") == true)
                {
                    return;
                }
            }
        }
        ////
        
        if (heldObject != null)
        {
            FireExtinguisherBehaviour extinguisher = heldObject.GetComponent<FireExtinguisherBehaviour>();

            if (extinguisher != null)
            {
                Vector3 dropPosition = transform.position + transform.forward * 1.2f;
                Collider[] hits = Physics.OverlapSphere(heldObject.transform.position, 2f);

                foreach (var col in hits)
                {
                    if (col.CompareTag("see"))
                    {
                        heldObject.transform.position = col.transform.position;
                        heldObject.transform.SetParent(null);
                        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                            rb.constraints = RigidbodyConstraints.None;
                        }
                        if (heldObject != null)
                        {
                            if (heldObject.GetComponent<ItemsBase>())
                            {
                                dealingController.SetItemGrabbed(null);
                            }
                        }
                        dropPosition = col.bounds.center;
                        heldObject.GetComponent<Collider>().enabled = true;
                        heldObject = null;
                        ShowText(false);

                        soundManager.ReproduceSound(dropExtinguisherSound);
                        return;
                    }
                }
            }
        }
        
        if (MySphereCast(out hit) == true)
        {
            return;
        }
        ///esto no

        if (heldObject.GetComponent<FoodBehaviour>() != null)
        {
            heldObject.GetComponent<Collider>().enabled = true;
            heldObject = null;
        }
    }
    /// 
    private void DetectObjects()
    {
        Debug.DrawRay(transform.position, transform.forward);
        RaycastHit hit;

        if (!MySphereCast(out hit))
        {
            interactionAdvise.gameObject.SetActive(false);
            interactionAdvise.GetComponent<SpriteRenderer>().sprite = defaultSprite;
            return;
        }
        if (ClientBehaviour.clientsBlocked)
        {
            if (hit.collider.CompareTag("MinigameTable"))
            {
                interactionAdvise.gameObject.SetActive(true);
                interactionAdvise.position = new Vector3(hit.collider.transform.position.x, interactionAdvise.position.y, hit.collider.transform.position.z);
                return;
            }
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("FireOut"))
            {
                interactionAdvise.gameObject.SetActive(true);
                interactionAdvise.position = new Vector3(hit.collider.transform.position.x, interactionAdvise.position.y, hit.collider.transform.position.z);
                return;
            }

            interactionAdvise.gameObject.SetActive(false);
            return;
        }
        if (MySphereCast(out hit) && heldObject == null && followingClient == null)
        {
            if (hit.collider.CompareTag("Client") || hit.collider.CompareTag("Grabbable") || hit.collider.CompareTag("FoodGenerator") || hit.collider.CompareTag("MinigameTable"))
            {
                GameObject gameObjectSeen = hit.collider.gameObject;

                ClientBehaviour cb = gameObjectSeen.GetComponent<ClientBehaviour>();

                if (cb != null)
                {
                    if (cb._canTakeOrder > 0 || cb._isOnTable)
                    {
                        interactionAdvise.gameObject.SetActive(false);
                        return;
                    }
                    if (cb.isInQueue)
                    {
                        GameObject first = spawner.GetFirstClient();

                        if (hit.collider.gameObject == first)
                        {
                            interactionAdvise.gameObject.SetActive(true);
                            interactionAdvise.position = new Vector3(
                                hit.collider.transform.position.x,
                                interactionAdvise.position.y,
                                hit.collider.transform.position.z
                            );

                            return;
                        }

                        interactionAdvise.gameObject.SetActive(false);
                        return;
                    }
                    interactionAdvise.gameObject.SetActive(true);
                    interactionAdvise.position = new Vector3(
                        hit.collider.transform.position.x,
                        interactionAdvise.position.y,
                        hit.collider.transform.position.z
                    );

                    return;
                }
                //eso no
                else if (hit.collider.CompareTag("MinigameTable"))
                {
                    if (GameManager.instance._miniGameManager.GetMinigameInfo((int)char.GetNumericValue(hit.transform.gameObject.name[0])).Request(heldObject) == false)
                    {
                        ShowMissing(missingFood);
                    }
                }
            
                interactionAdvise.gameObject.SetActive(true);
                interactionAdvise.position = new Vector3(hit.collider.transform.position.x, interactionAdvise.position.y, hit.collider.transform.position.z);

                return;
            }///
        }
        else if (heldObject != null)
        {
            if (heldObject.GetComponent<FireExtinguisherBehaviour>() != null)
            {
                if (MySphereCast(out hit))
                {
                    if (hit.collider.CompareTag("see"))
                    {
                        interactionAdvise.gameObject.SetActive(true);

                        interactionAdvise.position = new Vector3(
                            hit.collider.transform.position.x,
                            interactionAdvise.position.y,
                            hit.collider.transform.position.z
                        );

                        return;
                    }
                }
            }
            if (heldObject.GetComponent<FoodBehaviour>() != null)
            {
                if (MySphereCast(out hit))
                {
                    if (hit.collider.gameObject.GetComponent<TableOrder>() != null)
                    {
                        interactionAdvise.gameObject.SetActive(true);
                        interactionAdvise.position = new Vector3(hit.collider.transform.position.x, interactionAdvise.position.y, hit.collider.transform.position.z);
                        return;
                    }
                }
            }
            //no
            if (hit.collider.CompareTag("MinigameTable"))
            {
                if (GameManager.instance._miniGameManager.GetMinigameInfo((int)char.GetNumericValue(hit.transform.gameObject.name[0])).Request(heldObject) == false)
                {
                    ShowMissing(missingFood);
                }

                interactionAdvise.gameObject.SetActive(true);
                interactionAdvise.position = new Vector3(hit.collider.transform.position.x, interactionAdvise.position.y, hit.collider.transform.position.z);
                return;
            }
            ///
        }
        if (MySphereCast(out hit))
        {
            if (followingClient != null)
            {
                if (hit.collider.CompareTag("TablePoint") || hit.collider.CompareTag("MinigameTable"))
                {
                    interactionAdvise.gameObject.SetActive(true);
                    interactionAdvise.position = new Vector3(hit.collider.transform.position.x, interactionAdvise.position.y, hit.collider.transform.position.z);
                    return;
                }

                interactionAdvise.gameObject.SetActive(false);
                return;
            }
        }
        interactionAdvise.gameObject.SetActive(false);
        interactionAdvise.GetComponent<SpriteRenderer>().sprite = defaultSprite;
    }

    private void ShowMissing(Sprite spriteToShow)
    {
        interactionAdvise.GetComponent<SpriteRenderer>().sprite = spriteToShow;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position - transform.forward * 0.5f;
        Vector3 endPoint = origin + transform.forward * (distanceRaycast + 0.5f);
        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.DrawWireSphere(endPoint, sphereRadius);
        Gizmos.DrawLine(origin, endPoint);
    }
}