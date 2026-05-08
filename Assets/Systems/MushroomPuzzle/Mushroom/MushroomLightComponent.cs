using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Component pattern: handles cap glow via a Light and (optionally) an emissive
// material on the cap renderer. Mushroom orchestrator drives SetGlow / ClearGlow.

public class MushroomLightComponent : MonoBehaviour
{
    [SerializeField] private Light capLight;
    [SerializeField] private Renderer capRenderer;
    [SerializeField] private float emissiveIntensity = 2f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private MaterialPropertyBlock block;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        ClearGlow();
    }

    public void SetGlow(Color color)
    {
        if (capLight != null)
        {
            capLight.color = color;
            capLight.enabled = true;
        }
        if (capRenderer != null)
        {
            capRenderer.GetPropertyBlock(block);
            block.SetColor(EmissionColorId, color * emissiveIntensity);
            capRenderer.SetPropertyBlock(block);
        }
    }

    public void ClearGlow()
    {
        if (capLight != null) capLight.enabled = false;
        if (capRenderer != null)
        {
            capRenderer.GetPropertyBlock(block);
            block.SetColor(EmissionColorId, Color.black);
            capRenderer.SetPropertyBlock(block);
        }
    }
}
