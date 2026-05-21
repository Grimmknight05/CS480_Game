using UnityEngine;

public class LevelTestSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string spawnPointGroup = "PlayerStart";

    void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Assign player prefab in LevelTestSpawner.");
            return;
        }

        // Find spawn point
        SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        Vector3 spawnPos = Vector3.zero;
        foreach (var sp in spawns)
        {
            if (sp.Group == spawnPointGroup)
            {
                spawnPos = sp.Position;
                break;
            }
        }

        // Instantiate player
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.tag = "Player";

        // Tell camera to follow this new player
        OrbitalCamera cam = FindAnyObjectByType<OrbitalCamera>();
        if (cam != null) cam.playerRef = player.transform;

        // Optionally assign camera pivot
        PlayerControllerRefactored pc = player.GetComponent<PlayerControllerRefactored>();
        if (pc != null)
        {
            GameObject pivot = GameObject.Find("CameraPivot");
            if (pivot != null) pc.CameraPivot = pivot.transform;
        }

        // Destroy this spawner so it doesn't spawn another player later
        Destroy(gameObject);
    }
}