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
    [SerializeField] private float detectionDistance = 8f;
    [SerializeField] private float brakeSpeed = 25f;

    [Header("Pedestrian Detection")]
    [SerializeField] private float visionDistance = 20f;
    [SerializeField] private float visionRadius = 2.5f;
    [SerializeField] private LayerMask pedestrianLayer;

    [Header("Sensor")]
    [SerializeField] private CarSensor frontSensor;

    private bool pedestrianDetected;

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
        MoveTowardsTarget();
    }

    private void DetectPedestrian(Vector3 direction)
    {
        pedestrianDetected = false;

        float dynamicDistance = Mathf.Max(
            visionDistance,
            currentSpeed * 0.7f
        );

        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(
            origin,
            visionRadius,
            direction,
            out RaycastHit hit,
            dynamicDistance,
            pedestrianLayer))
        {
            ClientMovementE client =
                hit.collider.GetComponentInParent<ClientMovementE>();

            if (client != null)
            {
                pedestrianDetected = true;
            }
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null)
            return;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= arrivalDistance)
        {
            onReachedTarget?.Invoke();
            Destroy(gameObject);
            return;
        }

        direction.Normalize();

        // Detectamos al peatón usando la dirección REAL
        // hacia donde se mueve el auto.
        DetectPedestrian(direction);

        bool carAhead =
            IsCarAhead(direction);

        bool emergencyStop =
            frontSensor != null &&
            frontSensor.PedestrianInside;

        bool shouldBrake =
            pedestrianDetected ||
            emergencyStop ||
            carAhead;

        if (shouldBrake)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                brakeSpeed * Time.deltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                moveSpeed,
                brakeSpeed * Time.deltaTime
            );
        }

        transform.position +=
            direction * currentSpeed * Time.deltaTime;

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
}

