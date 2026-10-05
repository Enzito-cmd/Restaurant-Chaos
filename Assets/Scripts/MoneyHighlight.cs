using UnityEngine;

public class MoneyHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Color emissionColor = Color.yellow;

    [SerializeField, Range(0f, 10f)]
    private float emissionIntensity = 2f;

    private Renderer[] renderers;
    private Material[] materials;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        int materialCount = 0;

        foreach (Renderer rend in renderers)
        {
            materialCount += rend.materials.Length;
        }

        materials = new Material[materialCount];

        int index = 0;

        foreach (Renderer rend in renderers)
        {
            Material[] rendererMaterials = rend.materials;

            foreach (Material material in rendererMaterials)
            {
                materials[index] = material;

                PrepareMaterial(material);

                index++;
            }
        }

        SetHighlight(false);
    }

    private void PrepareMaterial(Material material)
    {
        if (material == null)
            return;


        Texture baseTexture = null;

        if (material.HasProperty("_BaseMap"))
        {
            baseTexture = material.GetTexture("_BaseMap");
        }
        else if (material.HasProperty("_MainTex"))
        {
            baseTexture = material.GetTexture("_MainTex");
        }


        if (baseTexture != null &&
            material.HasProperty("_EmissionMap"))
        {
            material.SetTexture(
                "_EmissionMap",
                baseTexture
            );
        }
    }

    public void SetHighlight(bool active)
    {
        if (materials == null)
            return;

        foreach (Material material in materials)
        {
            if (material == null)
                continue;

            SetEmission(material, active);
        }
    }

    private void SetEmission(Material material, bool active)
    {
        if (!material.HasProperty("_EmissionColor"))
            return;

        if (active)
        {
            material.EnableKeyword("_EMISSION");

            Color finalColor =
                emissionColor * emissionIntensity;

            material.SetColor(
                "_EmissionColor",
                finalColor
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