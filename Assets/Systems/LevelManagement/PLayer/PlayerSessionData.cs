using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Session/Player Session Data", fileName = "PlayerSessionData")]
public class PlayerSessionData : ScriptableObject
{
    [SerializeField] private SpawnSO lastCheckpointSpawn;
    [SerializeField] private bool hasCheckpoint;
    [SerializeField] private List<AreaSO> completedAreas = new List<AreaSO>();
    private AreaSO lastArea;  
    private static PlayerSessionData _instance;
    public static void SetInstance(PlayerSessionData data) => _instance = data;
    public static PlayerSessionData Instance => _instance;


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

    // --- Zone methods (if still needed) ---
    public void SetLastArea(AreaSO area) => lastArea = area;
    public AreaSO GetLastArea() => lastArea;
}