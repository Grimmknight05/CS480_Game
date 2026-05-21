using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointVolume : MonoBehaviour
{
    [SerializeField] private PlayerSessionData sessionData;
    [SerializeField] private SpawnSO checkpointSpawn;   // replaces Transform safeSpawnPoint
    [SerializeField] private AreaSO area;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = false;

    private bool consumed;

    void OnTriggerEnter(Collider other)
    {
        if (oneShot && consumed) return;
        if (!other.CompareTag(playerTag)) return;
        if (sessionData == null || checkpointSpawn == null) return;

        
        sessionData.SetCheckpointSpawn(checkpointSpawn);
        sessionData.SetLastArea(area);
        consumed = true;
        Debug.Log($"Checkpoint set to Spawn '{checkpointSpawn.name}' in area {area?.AreaId}");
    }
}