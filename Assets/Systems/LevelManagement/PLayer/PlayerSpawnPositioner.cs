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
        if (sessionData.TryGetCheckpointSpawn(out SpawnSO checkpointSpawn))
        {
            Vector3 pos = SpawnUtility.GetPosition(checkpointSpawn);
            SafeTeleport(pos);
            playerHealth?.RestoreFull();
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
