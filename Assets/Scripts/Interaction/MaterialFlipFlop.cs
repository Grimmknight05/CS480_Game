using System.Collections.Generic;
using UnityEngine;

public class MaterialFlipFlop : MonoBehaviour
{
    [System.Serializable]
    public class Target
    {
        public Renderer renderer;
        public Material materialA;
        public Material materialB;
    }

    [Header("Targets")]
    public List<Target> targets = new List<Target>();

    [Header("State")]
    public bool isFlipped;
    /*private void OnEnable()
    {
        ActivatorVisualBus.OnStateVisual += HandleState;
    }

    private void OnDisable()
    {
        ActivatorVisualBus.OnStateVisual -= HandleState;
    }*/
    public void SetFlipped(bool value)
    {
        isFlipped = value;

        foreach (var t in targets)
        {
            if (t.renderer == null) continue;

            Material matToApply = isFlipped ? t.materialB : t.materialA;
            t.renderer.material = matToApply;

            HandleEmission(t.renderer, isFlipped);
        }
    }

    public void Toggle()
    {
        SetFlipped(!isFlipped);
    }

    private void HandleEmission(Renderer rend, bool state)
    {
        Material mat = rend.material;

        if (state)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white * 2f);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }
    }
}