using UnityEngine;

public class CarSensor : MonoBehaviour
{
    public bool PedestrianInside { get; private set; }

    private int pedestriansCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (IsPedestrian(other))
        {
            pedestriansCount++;
            PedestrianInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPedestrian(other))
        {
            pedestriansCount--;

            if (pedestriansCount <= 0)
            {
                pedestriansCount = 0;
                PedestrianInside = false;
            }
        }
    }

    private bool IsPedestrian(Collider other)
    {
        // Detectar por Layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Pedestrian"))
        {
            return true;
        }

        // Detectar por Tag
        if (other.CompareTag("Pedestrian"))
        {
            return true;
        }

        // Detectar por el script
        if (other.GetComponentInParent<ClientMovementE>() != null)
        {
            return true;
        }

        return false;
    }
}