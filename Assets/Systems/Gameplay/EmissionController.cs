using UnityEngine;
using UnityEngine.Rendering;

public class MaterialController : MonoBehaviour, ILock
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
        ff.ObjectToChange.material = ff.state ? ff.targetMaterial : ff.startMaterial;
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
    private void OnDisable()
    {
        ResetAllMaterialsBypassLock();
    }
}
