using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Session/Player Session Data", fileName = "PlayerSessionData")]
public class PlayerSessionData : ScriptableObject
{
    [SerializeField] private SpawnSO lastCheckpointSpawn;
    [SerializeField] private bool hasCheckpoint;
    [SerializeField] private List<AreaSO> completedAreas = new List<AreaSO>();
    [SerializeField] private List<WorldSO> completedWorlds = new List<WorldSO>();
    private AreaSO lastArea;
    private static PlayerSessionData _instance;
    public static void SetInstance(PlayerSessionData data) => _instance = data;
    public static PlayerSessionData Instance => _instance;

    // Editor Persistence Protocol: SOs mutate permanently in Play Mode, so wipe
    // transient progression on load to guarantee a clean session start.
    private void OnEnable()
    {
        completedWorlds.Clear();
    }


    // --- Checkpoint methods ---
    public void SetCheckpointSpawn(SpawnSO spawn)
    {
        lastCheckpointSpawn = spawn;
        hasCheckpoint = true;
    }
    public bool TryGetCheckpointSpawn(out SpawnSO spawn)
    {
        spawn = lastCheckpointSpawn;
        return hasCheckpoint;
    }

    public void ClearCheckpoint()
    {
        lastCheckpointSpawn = null;
        hasCheckpoint = false;
    }



    // --- Area methods (accept AreaSO) ---
    public bool IsAreaCompleted(AreaSO area)
    {
        if (area == null) return false;
        return completedAreas.Contains(area);
    }

    public void CompleteArea(AreaSO area)
    {
        if (area == null) return;
        if (!completedAreas.Contains(area))
            completedAreas.Add(area);
    }

    public void ResetAllAreas() => completedAreas.Clear();

    // --- World methods (accept WorldSO) ---
    public bool IsWorldCompleted(WorldSO world)
    {
        if (world == null) return false;
        return completedWorlds.Contains(world);
    }

    public void CompleteWorld(WorldSO world)
    {
        if (world == null) return;
        if (!completedWorlds.Contains(world))
            completedWorlds.Add(world);
    }

    public void ResetAllWorlds() => completedWorlds.Clear();

    // --- Zone methods (if still needed) ---
    public void SetLastArea(AreaSO area) => lastArea = area;
    public AreaSO GetLastArea() => lastArea;
}