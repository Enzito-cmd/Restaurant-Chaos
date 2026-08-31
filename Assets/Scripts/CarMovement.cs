using UnityEngine;
using System;

public class CarMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Arrival")]
    [SerializeField] private float arrivalDistance = 1f;

    private Transform target;
    private Action onReachedTarget;

    public void SetTarget(
        Transform newTarget,
        Action reachedTargetCallback)
    {
        target = newTarget;
        onReachedTarget = reachedTargetCallback;
    }

    private void Update()
    {
        if (target == null)
            return;

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        // Llegó al final
        if (distance <= arrivalDistance)
        {
            onReachedTarget?.Invoke();
            Destroy(gameObject);
            return;
        }

        direction.Normalize();

        // Movimiento constante
        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        // Girar suavemente hacia adelante
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
    Quaternion.LookRotation(direction) *
    Quaternion.Euler(0f, -90f, 0f);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }
    }
}