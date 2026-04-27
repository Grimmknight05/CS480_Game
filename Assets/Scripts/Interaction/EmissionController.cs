using UnityEngine;

public class EmissionController : MonoBehaviour
{
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Color emissionColor = Color.cyan;
    [SerializeField] private float intensity = 2f;

    private Color originalEmission;
    private bool hadEmission;

    private void Awake()
    {
        if (targetMaterial.HasProperty("_EmissionColor"))
        {
            originalEmission = targetMaterial.GetColor("_EmissionColor");
        }

        hadEmission = targetMaterial.IsKeywordEnabled("_EMISSION");
    }

    public void EnableEmission()
    {
        targetMaterial.EnableKeyword("_EMISSION");
        targetMaterial.SetColor("_EmissionColor", emissionColor * intensity);
    }

    public void ResetMaterial()
    {
        if (!hadEmission)
            targetMaterial.DisableKeyword("_EMISSION");

        targetMaterial.SetColor("_EmissionColor", originalEmission);
    }

    private void OnDisable()
    {
        // Called when exiting play mode or object is disabled
        ResetMaterial();
    }
}