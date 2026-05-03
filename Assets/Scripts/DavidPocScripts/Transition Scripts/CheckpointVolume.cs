using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointVolume : MonoBehaviour
{
    [SerializeField] private PlayerSessionData sessionData;
    [SerializeField] private Transform safeSpawnPoint;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = false;

    private bool consumed;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: CheckpointVolume collider was not a trigger; forcing IsTrigger = true.", this);
            col.isTrigger = true;
        }

        if (safeSpawnPoint == null)
        {
            Debug.LogError($"{gameObject.name}: CheckpointVolume has no safeSpawnPoint assigned. Respawn coordinate would be invalid.", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (oneShot && consumed) return;
        if (!other.CompareTag(playerTag)) return;
        if (sessionData == null || safeSpawnPoint == null) return;

        sessionData.SetCheckpoint(safeSpawnPoint.position);
        consumed = true;
    }
}
