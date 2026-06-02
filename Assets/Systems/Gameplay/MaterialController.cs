using System.Collections.Generic;
using UnityEngine;

public class MaterialController : ResettableBehaviour, ILock
{
    [System.Serializable]
    public class flipFlop
    {
        public Renderer ObjectToChange;
        public Material startMaterial;
        public Material targetMaterial;
        public bool state = false;
    }

    [SerializeField] public flipFlop[] Object;
    [Header("Emission Blink")]
    [SerializeField] private bool autoEmissionBlink = false;
    [SerializeField] private Color blinkEmissionColor = Color.green;
    [SerializeField, Min(0f)] private float minEmission = 0.1f;
    [SerializeField, Min(0f)] private float maxEmission = 2.5f;
    [SerializeField, Min(0.01f)] private float emissionBlinkSpeed = 1.25f;
    [SerializeField] private bool includeChildRenderers = true;
    [SerializeField] private Material[] blinkOnlyMaterials;

    private bool _lockState = false;
    private MaterialPropertyBlock emissionPropertyBlock;
    private EmissionTarget[] emissionTargets;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private struct EmissionTarget
    {
        public Renderer Renderer;
        public int MaterialIndex;
    }

    public bool LockState 
    { 
        get { return _lockState; }
        set { _lockState = value; }
    }
    public void Lock(bool lockstate)
    {
        LockState = lockstate;
        Debug.Log($"Pusher is now {(lockstate ? "LOCKED" : "UNLOCKED")}");
    }
    
    public void ResetAllMaterialsForced()
    {
        // Reset all materials to startMaterial ignoring the lock state
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;
            ff.state = false;
            UpdateMaterial(ff);
        }
        _lockState = false; // optionally unlock after reset
    }
    void Start()
    {
        BuildEmissionTargets();
        InitAllMaterials();
    }

    private void Update()
    {
        if (!autoEmissionBlink || LockState) return;
        UpdateEmissionBlink();
    }
    
    public void ToggleAllMaterials()
    {
        IsLocked();
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;

            ff.state = !ff.state;
            UpdateMaterial(ff);
        }
    }
    public void SetAllMaterials(bool state)
    {
        IsLocked();
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;

            ff.state = state;
            UpdateMaterial(ff);
        }
    }
    public void SetAllMaterialsAndLock(bool state)
    {
        IsLocked();
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;

            ff.state = state;
            UpdateMaterial(ff);
        }
        _lockState = true;
    }
    public void UpdateAllMaterials()
    {
        IsLocked();
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;
            UpdateMaterial(ff);
        }
    }
    public void InitAllMaterials()
    {
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;
            UpdateMaterial(ff);
        }
    }
    public void ToggleMaterial(int index)
    {
        IsLocked();
        if (index < 0 || index >= Object.Length) return;

        flipFlop ff = Object[index];
        if (ff.ObjectToChange == null) return;

        ff.state = !ff.state;
        UpdateMaterial(ff);
    }
    public void SetMaterial(int index, bool state)
    {
        IsLocked();
        if (index < 0 || index >= Object.Length) return;

        flipFlop ff = Object[index];
        if (ff.ObjectToChange == null) return;

        ff.state = state;
        UpdateMaterial(ff);
    }


    private void UpdateMaterial(flipFlop ff)
    {
        Material material = ff.state ? ff.targetMaterial : ff.startMaterial;
        ApplyMaterial(ff.ObjectToChange, material);
    }

    private void ApplyMaterial(Renderer rootRenderer, Material material)
    {
        if (rootRenderer == null || material == null) return;

        rootRenderer.material = material;

        Renderer[] childRenderers = rootRenderer.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer childRenderer in childRenderers)
        {
            if (childRenderer == null || childRenderer == rootRenderer) continue;
            childRenderer.material = material;
        }
    }

    private void BuildEmissionTargets()
    {
        if (!autoEmissionBlink)
        {
            emissionTargets = System.Array.Empty<EmissionTarget>();
            return;
        }

        Renderer[] renderers = includeChildRenderers
            ? GetComponentsInChildren<Renderer>(true)
            : GetComponents<Renderer>();

        List<EmissionTarget> targets = new List<EmissionTarget>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            Material[] materials = renderer.sharedMaterials;
            for (int index = 0; index < materials.Length; index++)
            {
                Material material = materials[index];
                if (material == null || !ShouldBlinkMaterial(material)) continue;

                material.EnableKeyword("_EMISSION");
                targets.Add(new EmissionTarget { Renderer = renderer, MaterialIndex = index });
            }
        }

        emissionTargets = targets.ToArray();
    }

    private void UpdateEmissionBlink()
    {
        if (emissionTargets == null) BuildEmissionTargets();
        if (emissionTargets == null || emissionTargets.Length == 0) return;

        if (emissionPropertyBlock == null)
            emissionPropertyBlock = new MaterialPropertyBlock();

        float low = Mathf.Min(minEmission, maxEmission);
        float high = Mathf.Max(minEmission, maxEmission);
        float pulse = (Mathf.Sin(Time.time * emissionBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        Color emissionColor = blinkEmissionColor * Mathf.Lerp(low, high, pulse);

        foreach (EmissionTarget target in emissionTargets)
        {
            if (target.Renderer == null) continue;

            emissionPropertyBlock.Clear();
            target.Renderer.GetPropertyBlock(emissionPropertyBlock, target.MaterialIndex);
            emissionPropertyBlock.SetColor(EmissionColorId, emissionColor);
            target.Renderer.SetPropertyBlock(emissionPropertyBlock, target.MaterialIndex);
        }
    }

    private bool ShouldBlinkMaterial(Material material)
    {
        if (blinkOnlyMaterials == null || blinkOnlyMaterials.Length == 0)
            return true;

        foreach (Material blinkMaterial in blinkOnlyMaterials)
        {
            if (material == blinkMaterial)
                return true;
        }

        return false;
    }

    public void ResetAllMaterials()
    {
        IsLocked();
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;
            ff.state = false;
            UpdateMaterial(ff);
        }
    }
    public void ResetAllMaterialsBypassLock()
    {
        IsLocked();
        foreach (flipFlop ff in Object)
        {
            if (ff.ObjectToChange == null) continue;
            ff.state = false;
            UpdateMaterial(ff);
        }
    }
    void IsLocked()
    {
        if (LockState)
        {
            Debug.LogWarning("EmissionController is locked!");
            return;
        }
    }
    // ResettableBehaviour implementation
    protected override void ResetInternal()
    {
        ResetAllMaterialsBypassLock();
        _lockState = false;
    }
}
