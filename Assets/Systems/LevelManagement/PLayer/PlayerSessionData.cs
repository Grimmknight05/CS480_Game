using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Session/Player Session Data", fileName = "PlayerSessionData")]
public class PlayerSessionData : ScriptableObject
{
    [SerializeField] private Vector3 lastCheckpoint;
    [SerializeField] private bool hasCheckpoint;
    [SerializeField] private List<string> completedAreas = new List<string>(); // stores areaId strings
    private string lastZone;

    // Optional static instance (if you still use it)
    private static PlayerSessionData _instance;
    public static void SetInstance(PlayerSessionData data) => _instance = data;
    public static PlayerSessionData Instance => _instance;

    void OnEnable()
    {
        // Reset checkpoint for a clean session each play mode entry
        hasCheckpoint = false;
        lastCheckpoint = Vector3.zero;
    }

    // --- Checkpoint methods ---
    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
        hasCheckpoint = true;
    }

    public void ClearCheckpoint()
    {
        lastCheckpoint = Vector3.zero;
        hasCheckpoint = false;
    }

    public bool TryGetCheckpoint(out Vector3 pos)
    {
        pos = lastCheckpoint;
        return hasCheckpoint;
    }

    // --- Area methods (accept AreaSO) ---
    public bool IsAreaCompleted(AreaSO area)
    {
        if (area == null) return false;
        return completedAreas.Contains(area.AreaId);
    }

    public void CompleteArea(AreaSO area)
    {
        if (area == null) return;
        string id = area.AreaId;
        if (!completedAreas.Contains(id))
            completedAreas.Add(id);
    }

    // Optional: raw string methods (for legacy or direct use)
    public bool IsAreaCompleted(string areaId) => completedAreas.Contains(areaId);
    public void CompleteArea(string areaId)
    {
        if (!completedAreas.Contains(areaId))
            completedAreas.Add(areaId);
    }

    public void ResetAllAreas() => completedAreas.Clear();

    // --- Zone methods (if still needed) ---
    public void SetLastZone(string zone) => lastZone = zone;
    public string GetLastZone() => lastZone;
}