using UnityEngine;
using System.Collections;

public class LevelBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private SpawnSO defaultSpawnSO;

    private void Start()
    {
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return null; // wait for LevelManager to initialize

        bool isHub = LevelManager.Instance != null &&
                     LevelManager.Instance.CurrentWorld != null &&
                     LevelManager.Instance.CurrentWorld.isHubWorld;

        SpawnPlayer(isHub);
    }

    private void SpawnPlayer(bool ignoreCheckpoint)
    {
        Vector3 spawnPos = ignoreCheckpoint ? GetDefaultSpawnPoint() : GetSpawnPosition();

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.tag = "Player";

        // Assign camera
        OrbitalCamera cam = FindFirstObjectByType<OrbitalCamera>();
        if (cam != null) cam.playerRef = player.transform;

        // Assign camera pivot if needed
        PlayerControllerRefactored controller = player.GetComponent<PlayerControllerRefactored>();
        if (controller != null)
        {
            GameObject pivot = GameObject.Find("CameraPivot");
            if (pivot != null) controller.CameraPivot = pivot.transform;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (GameProgress.TryGetCheckpointSpawn(out SpawnSO checkpointSpawn))
            return SpawnUtility.GetPosition(checkpointSpawn);
        return GetDefaultSpawnPoint();
    }

    private Vector3 GetDefaultSpawnPoint()
    {
        if (defaultSpawnSO == null)
        {
            Debug.LogError("LevelBootstrapper: No defaultSpawnSO assigned!");
            return Vector3.zero;
        }

        SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var sp in spawns)
            if (sp.SpawnSO == defaultSpawnSO)
                return sp.Position;

        Debug.LogWarning($"No SpawnPoint with SpawnSO '{defaultSpawnSO.name}' found. Using zero.");
        return Vector3.zero;
    }
}