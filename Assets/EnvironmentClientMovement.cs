using UnityEngine;

public class EnvironmentClientMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    private Transform[] pathPoints;
    private int currentPoint;

    public void SetPath(Transform[] newPath)
    {
        pathPoints = newPath;
        currentPoint = 0;
    }

    private void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        if (currentPoint >= pathPoints.Length)
        {
            Destroy(gameObject);
            return;
        }

        Transform target = pathPoints[currentPoint];

        if (target == null)
        {
            currentPoint++;
            return;
        }

        Vector3 direction = target.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        // Llegó al punto
        if (distance <= 0.2f)
        {
            currentPoint++;
            return;
        }

        direction.Normalize();

        // Movimiento
        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        // Rotación
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}