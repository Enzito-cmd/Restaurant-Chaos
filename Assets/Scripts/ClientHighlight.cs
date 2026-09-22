using UnityEngine;

public class ClientHighlight : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float brightness = 2f;

    private Renderer[] renderers;
    private Color[] originalColors;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        int cantidadMateriales = 0;

        for (int i = 0; i < renderers.Length; i++)
        {
            cantidadMateriales += renderers[i].materials.Length;
        }

        originalColors = new Color[cantidadMateriales];

        int index = 0;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;

            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty("_BaseColor"))
                {
                    originalColors[index] =
                        materials[j].GetColor("_BaseColor");
                }

                index++;
            }
        }
    }

    public void SetHighlight(bool active)
    {
        int index = 0;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;

            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty("_BaseColor"))
                {
                    if (active)
                    {
                        Color color = highlightColor * brightness;

                        materials[j].SetColor("_BaseColor", color);
                    }
                    else
                    {
                        materials[j].SetColor(
                            "_BaseColor",
                            originalColors[index]
                        );
                    }
                }

                index++;
            }
        }
    }
}