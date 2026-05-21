using UnityEngine;
using UnityEngine.Rendering;

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
    private bool _lockState = false;
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
        InitAllMaterials();
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
