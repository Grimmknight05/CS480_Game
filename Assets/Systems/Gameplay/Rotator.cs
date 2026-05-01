using UnityEngine;

public class Rotator : MonoBehaviour, ILock
{
    [System.Serializable]
    public class Rotation
    {
        public GameObject ObjectToRotate;
        public Vector3 LocalRotation = Vector3.zero;
        public float RotationSpeed = 1.0f;
        public float Duration = 1.5f;
        public AnimationCurve Ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public ParticleSystem VFX;

        [HideInInspector] public float Timer = 0f;
        [HideInInspector] public Quaternion StartRotation;
        [HideInInspector] public Quaternion TargetRotation;
        [HideInInspector] public bool IsAnimating = false;
    }

    [SerializeField] public Rotation[] Rotations;

    //Lock handle
    private bool _lockState = false;
    public bool LockState 
    { 
        get { return _lockState; }
        set { _lockState = value; }
    }
    public void Lock(bool lockstate)
    {
        LockState = lockstate;
        Debug.Log($"Rotator is now {(lockstate ? "LOCKED" : "UNLOCKED")}");
    }

    void Start()
    {
        InitializeRotations();
    }
    public void InitializeRotations()
    {
        foreach (var rotation in Rotations)
        {
            if (rotation.ObjectToRotate != null)
            {
                rotation.StartRotation = rotation.ObjectToRotate.transform.localRotation;
                //rotation.TargetRotation = Quaternion.Euler(rotation.LocalRotation);
                rotation.Timer = 0f;
                rotation.IsAnimating = false;
            }
        }
    }
    void Update()
    {
        UpdateRotations();
    }
    public void RotateObject(GameObject targetObject)
    {   
        IsLocked();
        foreach (var rotation in Rotations)
        {
            if (rotation.ObjectToRotate == targetObject)
            {
                RotateObject(rotation);
                return;
            }
        }
        Debug.LogWarning($"Object {targetObject.name} not found in Rotations!");
    }
    public void RotateAll()
    {
        IsLocked();
        foreach (var rotation in Rotations)
        {
            RotateObject(rotation);
        }
    }
    public void RotateAllAndLock()
    {
        IsLocked();
        foreach (var rotation in Rotations)
        {
            RotateObject(rotation);
        }
        _lockState = true;
    }
    public void RotateByIndex(int index)
    {
        IsLocked();
        if (index >= 0 && index < Rotations.Length)
        {
            RotateObject(Rotations[index]);
        }
        else
        {
            Debug.LogWarning($"Rotation index {index} is out of range!");
        }
    }
    public void ResetRotation(int index)
    {
        IsLocked();
        if (index >= 0 && index < Rotations.Length)
        {
            Rotations[index].Timer = 0f;
            Rotations[index].IsAnimating = false;
            if (Rotations[index].ObjectToRotate != null)
            {
                Rotations[index].ObjectToRotate.transform.localRotation = Rotations[index].StartRotation;
            }
        }
    }
    public void ResetAllRotations()
    {
        IsLocked();
        for (int i = 0; i < Rotations.Length; i++)
        {
            ResetRotation(i);
        }
    }

    private void RotateObject(Rotation rotation)
    {
        rotation.Timer = 0f;
        rotation.IsAnimating = true;
        rotation.StartRotation = rotation.ObjectToRotate.transform.localRotation;
        rotation.TargetRotation = rotation.StartRotation * Quaternion.Euler(rotation.LocalRotation);
    }

    private void UpdateRotations()
    {
        foreach (var rotation in Rotations)
        {
            if (!rotation.IsAnimating || rotation.ObjectToRotate == null) continue;

            rotation.Timer += Time.deltaTime * rotation.RotationSpeed;
            float progress = Mathf.Clamp01(rotation.Timer / rotation.Duration);
            float easedProgress = rotation.Ease.Evaluate(progress);

            rotation.ObjectToRotate.transform.localRotation = 
                Quaternion.Lerp(rotation.StartRotation, rotation.TargetRotation, easedProgress);

            if (rotation.Timer >= rotation.Duration)
            {
                rotation.IsAnimating = false;
                if (rotation.VFX != null)
                {
                    rotation.VFX.Play();
                }
            }
        }
    }
    void IsLocked()
    {
        if (LockState)
        {
            Debug.LogWarning("Cannot rotate - Rotator is locked!");
            return;
        }
    }
}
