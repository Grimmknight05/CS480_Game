using UnityEngine;

public class PlayerSpawnPositioner : MonoBehaviour
{
    [SerializeField] private PlayerSessionData sessionData;

    void Start()
    {
        if (sessionData == null)
        {
            Debug.LogError($"{gameObject.name}: PlayerSpawnPositioner has no sessionData assigned.", this);
            return;
        }

        if (sessionData.TryGetCheckpoint(out Vector3 pos))
        {
            SafeTeleport(pos);
        }
        else
        {
            sessionData.SetCheckpoint(transform.position);
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
