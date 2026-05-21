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

    /*public IEnumerator Execute(LevelManager manager)
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
    }*/
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
    }

}