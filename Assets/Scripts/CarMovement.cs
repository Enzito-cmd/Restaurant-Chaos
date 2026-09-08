using UnityEngine;
using System;

public class CarMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Arrival")]
    [SerializeField] private float arrivalDistance = 1f;

    [Header("Traffic")]
    [SerializeField] private float detectionDistance = 6f;
    [SerializeField] private float safeDistance = 3f;
    [SerializeField] private float brakeSpeed = 15f;

    private Transform target;
    private Action onReachedTarget;

    private float currentSpeed;

    public void SetTarget(
        Transform newTarget,
        Action reachedTargetCallback)
    {
        target = newTarget;
        onReachedTarget = reachedTargetCallback;

        currentSpeed = moveSpeed;
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

        // ==========================================
        // LLEGÓ AL FINAL
        // ==========================================

        if (distance <= arrivalDistance)
        {
            onReachedTarget?.Invoke();
            Destroy(gameObject);
            return;
        }

        direction.Normalize();

        // ==========================================
        // DETECTAR AUTO DE ADELANTE
        // ==========================================

        bool carAhead = IsCarAhead(direction);

        if (carAhead)
        {
            // Frenar suavemente
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                brakeSpeed * Time.deltaTime
            );
        }
        else
        {
            // Volver a acelerar
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                moveSpeed,
                brakeSpeed * Time.deltaTime
            );
        }

        // ==========================================
        // MOVIMIENTO
        // ==========================================

        transform.position +=
            direction * currentSpeed * Time.deltaTime;

        // ==========================================
        // ROTACIÓN
        // ==========================================

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

    private bool IsCarAhead(Vector3 direction)
    {
        RaycastHit hit;

        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(
            origin,
            direction,
            out hit,
            detectionDistance))
        {
            CarMovement otherCar =
                hit.collider.GetComponentInParent<CarMovement>();

            if (otherCar != null &&
                otherCar != this)
            {
                return true;
            }
        }

        return false;
    }
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
        currentSpeed = speed;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        Gizmos.DrawRay(
            origin,
            transform.forward * detectionDistance
        );
    }
}