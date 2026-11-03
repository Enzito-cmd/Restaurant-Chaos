using UnityEngine;

public class DarkenCharacter : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float darkness = 0.5f;

    private Renderer[] renderers;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    mat.color = c * darkness;
                }
            }
        }
    }
}