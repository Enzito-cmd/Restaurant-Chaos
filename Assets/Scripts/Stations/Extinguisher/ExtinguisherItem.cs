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

    private void OnDisable()
    {
        SoundManager.Instance?.StopLoopingSound(SoundType.ExtinguisherSpray);
    }

    public void UseExtinguisher()
    {
        SoundManager.Instance?.StartLoopingSound(SoundType.ExtinguisherSpray);

        if (foamParticles != null)
        {
            foamParticles.Play();
        }
    }
    public void StopExtinguisher()
    {
        SoundManager.Instance?.StopLoopingSound(SoundType.ExtinguisherSpray);

        if (foamParticles != null)
        {
            foamParticles.Stop();
        }
    }
}