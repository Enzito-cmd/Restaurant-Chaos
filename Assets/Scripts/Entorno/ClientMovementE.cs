using UnityEngine;
using UnityEngine.AI;
using System;

public class ClientMovementE : MonoBehaviour
{
    private Transform target;
    private Action onArrived;

    [SerializeField] private float speed = 2f;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetTarget(Transform newTarget, Action callback)
    {
        target = newTarget;
        onArrived = callback;
    }

    private void Update()
    {
        if (target == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        Vector3 direction =
            (target.position - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            onArrived?.Invoke();
            Destroy(gameObject);
        }
    }
}

