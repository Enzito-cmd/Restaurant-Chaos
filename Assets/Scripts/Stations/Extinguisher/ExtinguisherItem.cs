using UnityEngine;

public class ExtinguisherItem : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem foamParticles;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseExtinguisher();
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            StopExtinguisher();
        }
    }

    public void UseExtinguisher()
    {
        if (foamParticles != null)
        {
            foamParticles.Play();
        }
    }
    public void StopExtinguisher()
    {
        if (foamParticles != null)
        {
            foamParticles.Stop();
        }
    }
}