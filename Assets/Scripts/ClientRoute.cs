using UnityEngine;

public class ClientRoute : MonoBehaviour
{
    [Header("Route")]
    public Transform spawnPoint;

    public Transform waitPoint;

    public Transform destinationPoint;

    [Header("Street Detection")]
    public Transform carCheckPoint;

    public float carDetectionRadius = 4f;

    public LayerMask carLayer;

    public bool IsCarNearby()
    {
        if (carCheckPoint == null)
            return false;

        return Physics.CheckSphere(
            carCheckPoint.position,
            carDetectionRadius,
            carLayer
        );
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (carCheckPoint == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            carCheckPoint.position,
            carDetectionRadius
        );
    }

#endif
}