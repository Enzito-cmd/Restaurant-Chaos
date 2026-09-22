using System.Collections.Generic;
using UnityEngine;

public class ClientHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Color emissionColor = Color.yellow;

    [SerializeField, Range(0f, 10f)]
    private float emissionIntensity = 1.5f;

    private Renderer[] renderers;

    private readonly List<Material> materials = new List<Material>();


    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        PrepareMaterials();

        SetHighlight(false);
    }


    private void PrepareMaterials()
    {
        materials.Clear();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;
            Material[] rendererMaterials = renderer.materials;

            foreach (Material material in rendererMaterials)
            {
                if (material == null)
                    continue;

                materials.Add(material);

                PrepareEmission(material);
            }
        }
    }


    private void PrepareEmission(Material material)
    {
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