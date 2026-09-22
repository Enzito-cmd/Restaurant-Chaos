using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ClientAI : MonoBehaviour
{
    private enum State
    {
        GoingToWaitPoint,
        WaitingForStreet,
        GoingToDestination
    }

    private NavMeshAgent agent;

    private ClientRoute route;

    private State state;

    [Header("Arrival")]
    [SerializeField]
    private float arrivalDistance = 0.3f;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    public void Initialize(ClientRoute newRoute)
    {
        route = newRoute;

        state = State.GoingToWaitPoint;

        agent.isStopped = false;

        agent.SetDestination(
            route.waitPoint.position
        );
    }


    private void Update()
    {
        if (route == null)
            return;

        switch (state)
        {
            case State.GoingToWaitPoint:

                UpdateGoingToWaitPoint();
                break;


            case State.WaitingForStreet:

                UpdateWaitingForStreet();
                break;


            case State.GoingToDestination:

                UpdateGoingToDestination();
                break;
        }
    }


    // =============================================
    // IR HACIA LA CALLE
    // =============================================

    private void UpdateGoingToWaitPoint()
    {
        if (!HasReachedDestination())
            return;

        state = State.WaitingForStreet;

        agent.isStopped = true;

        agent.velocity = Vector3.zero;
    }


    // =============================================
    // ESPERAR AUTOS
    // =============================================

    private void UpdateWaitingForStreet()
    {
        bool carNearby =
            route.IsCarNearby();

        // 🚗 Hay auto
        if (carNearby)
        {
            agent.isStopped = true;
            return;
        }

        // ✅ No hay auto
        agent.isStopped = false;

        agent.SetDestination(
            route.destinationPoint.position
        );

        state =
            State.GoingToDestination;
    }


    // =============================================
    // DESTINO FINAL
    // =============================================

    private void UpdateGoingToDestination()
    {
        if (!HasReachedDestination())
            return;

        Destroy(gameObject);
    }


    // =============================================
    // COMPROBAR LLEGADA
    // =============================================

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return false;

        return agent.remainingDistance <=
               Mathf.Max(
                   arrivalDistance,
                   agent.stoppingDistance
               );
    }
}