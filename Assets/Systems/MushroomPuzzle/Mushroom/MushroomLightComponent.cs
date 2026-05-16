using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Component pattern: handles mushroom appearance by swapping materials.
// Assign an "active" material (with bloom/emission already authored in it)
// and this component swaps to it while active, then restores the original.

public class MushroomLightComponent : MonoBehaviour
{
    [SerializeField] private Renderer capRenderer;
    [Tooltip("Which material slot on the renderer to swap.")]
    [SerializeField] private int materialIndex = 0;
    [Tooltip("Material used while mushroom is active/solved. Put your bloom-enabled material here.")]
    [SerializeField] private Material activeMaterial;

    private Material[] runtimeMaterials;
    private Material dormantMaterial;

    private void Awake()
    {
        if (capRenderer == null) return;
        runtimeMaterials = capRenderer.materials;
        if (runtimeMaterials != null && materialIndex >= 0 && materialIndex < runtimeMaterials.Length)
        {
            dormantMaterial = runtimeMaterials[materialIndex];
        }
        ClearGlow();
    }

    public void SetGlow(Color color)
    {
        ApplyMaterial(activeMaterial != null ? activeMaterial : dormantMaterial);
    }

    public void ClearGlow()
    {
        ApplyMaterial(dormantMaterial);
    }

    private void ApplyMaterial(Material target)
    {
        if (capRenderer == null || target == null) return;

        if (runtimeMaterials == null || runtimeMaterials.Length == 0)
            runtimeMaterials = capRenderer.materials;

        if (runtimeMaterials == null || materialIndex < 0 || materialIndex >= runtimeMaterials.Length)
            return;

        runtimeMaterials[materialIndex] = target;
        capRenderer.materials = runtimeMaterials;
    }
}
