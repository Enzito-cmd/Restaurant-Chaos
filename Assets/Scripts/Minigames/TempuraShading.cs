using UnityEngine;

public static class TempuraShading
{
    private static readonly int RawToBreadedId = Shader.PropertyToID("_RawToBreaded");
    private static readonly int BreadedToFriedId = Shader.PropertyToID("_BreadedToFried");
    private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();

    public static void Setup(GameObject tempura, Material material)
    {
        if (material == null)
        {
            return;
        }

        foreach (Renderer renderer in tempura.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterial = material;
        }
    }

    public static void Apply(GameObject tempura, float rawToBreaded, float breadedToFried)
    {
        foreach (Renderer renderer in tempura.GetComponentsInChildren<Renderer>())
        {
            renderer.GetPropertyBlock(block);
            block.SetFloat(RawToBreadedId, rawToBreaded);
            block.SetFloat(BreadedToFriedId, breadedToFried);
            renderer.SetPropertyBlock(block);
        }
    }
}
