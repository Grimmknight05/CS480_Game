using UnityEngine;

public class PlayerSpawnPositioner : MonoBehaviour
{
    [SerializeField] private PlayerSessionData sessionData;
    [SerializeField] private LevelResetChannelSO resetChannel;
    [SerializeField] private PlayerHealth playerHealth;
    void OnEnable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised += RespawnAtCheckpoint;
    }

    void OnDisable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised -= RespawnAtCheckpoint;
    }

    void Start()
    {

    }
    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("PlayerSpawnPositioner: PlayerHealth component missing on this GameObject.");
    }
    public void RespawnAtCheckpoint()
    {
        if (sessionData == null) return;
        if (sessionData.TryGetCheckpoint(out Vector3 pos))
        {
            SafeTeleport(pos);
            
            // Restore health
            if (playerHealth != null)
                playerHealth.RestoreFull();
            else
                Debug.LogWarning("PlayerSpawnPositioner: No PlayerHealth reference, health not restored.");
        }
    }

    void SafeTeleport(Vector3 pos)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Zero motion before the move so the next physics tick doesn't carry over
            // a fall velocity from the previous life into the respawn point.
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
        }
        transform.position = pos;
    }
}
