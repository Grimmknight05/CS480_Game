using UnityEngine;

public class Respawnable : ResettableBehaviour
{
    [Tooltip("Optional. If null, captures transform.position at Awake.")]
    [SerializeField] private Transform spawnPoint;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Rigidbody rb;
    private bool resetLocked;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }
        else
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }
    }

    public void ResetToSpawn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPosition;
            rb.rotation = spawnRotation;
        }
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
    }

    public void LockReset() => resetLocked = true;
    public void UnlockReset() => resetLocked = false;
    protected override void ResetInternal()
    {
        if (resetLocked) return;
        ResetToSpawn();
    }
}
