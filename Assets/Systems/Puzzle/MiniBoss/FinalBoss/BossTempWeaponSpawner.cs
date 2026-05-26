using UnityEngine;

public class BossTempWeaponSpawner : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField] private SpawnerSO spawnerId;

    [Header("Spawning")]
    public GameObject tempWeaponPickupPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 2f;

    [Header("Visual Lock Effect (Scene Reference)")]
    public GameObject lockVisual;   // this GameObject will be enabled when locked, disabled when unlocked

    [Header("Events")]
    [SerializeField] private TempWeaponDropChannelSO tempWeaponDropChannel;

    private GameObject currentPickup;
    private bool isLocked = true;
    private bool isRespawning = false;

    public bool IsLocked => isLocked;

    private void OnEnable()
    {
        if (tempWeaponDropChannel != null)
            tempWeaponDropChannel.OnDrop += HandleTempWeaponDropped;
    }

    private void OnDisable()
    {
        if (tempWeaponDropChannel != null)
            tempWeaponDropChannel.OnDrop -= HandleTempWeaponDropped;
    }

    private void Start()
    {
        if (spawnerId != null)
            TempWeaponSpawnerRegistry.Register(spawnerId, this);

        SetLockedVisual(true);
        SpawnPickupIfAllowed();
    }

    private void OnDestroy()
    {
        if (spawnerId != null)
            TempWeaponSpawnerRegistry.Unregister(spawnerId);
    }

    private void SetLockedVisual(bool locked)
    {
        if (lockVisual != null)
            lockVisual.SetActive(locked);
    }

    public void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;
        SetLockedVisual(false);
        SpawnPickupIfAllowed();
    }

    public void Lock()
    {
        if (isLocked) return;
        isLocked = true;
        SetLockedVisual(true);
        if (currentPickup != null)
            Destroy(currentPickup);
    }

    private void SpawnPickupIfAllowed()
    {
        if (isRespawning) return;
        if (!isLocked && currentPickup == null && tempWeaponPickupPrefab != null && spawnPoint != null)
        {
            currentPickup = Instantiate(tempWeaponPickupPrefab, spawnPoint.position, spawnPoint.rotation);
            var pickup = currentPickup.GetComponent<TempWeaponPickup>();
            if (pickup != null)
                pickup.OnPickup += HandlePickupTaken;
        }
    }

    private void HandlePickupTaken(TempWeaponPickup pickup)
    {
        pickup.OnPickup -= HandlePickupTaken;
        currentPickup = null;
    }

    private void HandleTempWeaponDropped()
    {
        if (!isLocked && currentPickup == null)
            RespawnPickup();
    }

    public void RespawnPickup()
    {
        if (isRespawning) return;
        isRespawning = true;
        Invoke(nameof(RespawnNow), respawnDelay);
    }

    private void RespawnNow()
    {
        isRespawning = false;
        SpawnPickupIfAllowed();
    }
}