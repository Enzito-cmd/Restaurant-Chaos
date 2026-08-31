using UnityEngine;

public class ClientHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Color emissionColor = Color.yellow;
    [SerializeField] private float emissionIntensity = 2f;

    private Renderer[] renderers;
    private Material[] materials;

    private void Awake()
    {
        // Incluye MeshRenderer y SkinnedMeshRenderer
        renderers = GetComponentsInChildren<Renderer>();

        materials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;

            // Apagado inicialmente
            SetEmission(materials[i], false);
        }
    }

    public void SetHighlight(bool active)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
                continue;

            SetEmission(materials[i], active);
        }
    }

    private void SetEmission(Material material, bool active)
    {
        if (!material.HasProperty("_EmissionColor"))
            return;

        if (active)
        {
            material.EnableKeyword("_EMISSION");

            material.SetColor(
                "_EmissionColor",
                emissionColor * emissionIntensity
            );
        }
        else
        {
            material.SetColor(
                "_EmissionColor",
                Color.black
            );

            material.DisableKeyword("_EMISSION");
        }
    }
}