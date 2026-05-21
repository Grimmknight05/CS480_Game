using UnityEngine;

public static class SpawnUtility
{
    public static Vector3 GetPosition(SpawnSO spawnSO)
    {
        if (spawnSO == null) return Vector3.zero;
        var point = Object.FindFirstObjectByType<SpawnPoint>();
        // Or find all and match reference
        var matches = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var sp in matches)
            if (sp.SpawnSO == spawnSO)
                return sp.Position;
        Debug.LogError($"No SpawnPoint with SpawnSO '{spawnSO.name}' found in scene.");
        return Vector3.zero;
    }
}