using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadWorldCommand : ISceneCommand
{
    private WorldSO world;

    public LoadWorldCommand(WorldSO world)
    {
        this.world = world;
    }

    public IEnumerator Execute(LevelManager manager)
    {
        if (manager.IsLoading) yield break;

        manager.RaiseLoadStarted(world);
        world.OnWillLoad(manager.Session);

        AsyncOperation op = SceneManager.LoadSceneAsync(world.sceneName);
        while (!op.isDone) yield return null;

        manager.SetCurrentWorld(world);
        world.OnDidLoad(manager.Session);
        manager.RaiseLoadCompleted(world);
        yield return null; // wait one frame

        // Instantiate player from prefab
        Vector3 spawnPos = GetSpawnPosition(manager);
        GameObject player = Object.Instantiate(manager.PlayerPrefab, spawnPos, Quaternion.identity);
        player.tag = "Player";

        //Settings
        PlayerControllerRefactored playerController = player.GetComponent<PlayerControllerRefactored>();
        if (playerController != null)
        {
            // Find the camera pivot by name (or by tag/component)
            GameObject pivotObj = GameObject.Find("CameraPivot");
            if (pivotObj != null)
            {
                playerController.CameraPivot = pivotObj.transform;   // you need to expose this field
                Debug.Log("Assigned camera pivot to player controller");
            }
            else
            {
                Debug.LogError("CameraPivot not found in scene!");
            }
        }
        if (playerController != null && world.overrideMovementMode)
        {
            playerController.SetMovementMode(world.defaultMovementMode);
            Debug.Log($"Set player movement mode to {world.defaultMovementMode} for level {world.sceneName}");
        }
        OrbitalCamera cam = Object.FindFirstObjectByType<OrbitalCamera>();
        if (cam != null)
        {
            cam.playerRef = player.transform;
            Debug.Log("Assigned player to OrbitalCamera");
        }
    }
    private Vector3 GetSpawnPosition(LevelManager manager)
    {
        // Priority 1: Checkpoint from PlayerSessionData
        if (manager.Session.TryGetCheckpoint(out Vector3 checkpoint))
        {
            Debug.Log($"Using checkpoint at {checkpoint}");
            return checkpoint;
        }

        // Priority 2: Find a SpawnPoint with group "PlayerStart" (or any group you decide)
        SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.Group == "PlayerStart")   // or maybe "HubStart", "LevelStart", etc.
            {
                Debug.Log($"Using SpawnPoint '{sp.name}' at {sp.Position}");
                return sp.Position;
            }
        }

        // Priority 3: Fallback
        Debug.LogWarning($"No PlayerStart spawn point or checkpoint found. Using (0,0,0).");
        return Vector3.zero;
    }
}