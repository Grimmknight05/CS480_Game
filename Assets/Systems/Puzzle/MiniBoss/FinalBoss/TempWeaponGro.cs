using System.Collections.Generic;
using UnityEngine;

public static class TempWeaponSpawnerGroupRegistry
{
    private static Dictionary<SpawnerSO, List<BossTempWeaponSpawner>> groupMap = new();

    public static void RegisterToGroup(SpawnerSO group, BossTempWeaponSpawner spawner)
    {
        if (group == null || spawner == null) return;
        if (!groupMap.ContainsKey(group))
            groupMap[group] = new List<BossTempWeaponSpawner>();
        if (!groupMap[group].Contains(spawner))
            groupMap[group].Add(spawner);
    }

    public static void UnregisterFromGroup(SpawnerSO group, BossTempWeaponSpawner spawner)
    {
        if (group == null || spawner == null) return;
        if (groupMap.TryGetValue(group, out var list))
            list.Remove(spawner);
    }

    public static List<BossTempWeaponSpawner> GetSpawnersInGroup(SpawnerSO group)
    {
        if (group == null) return new List<BossTempWeaponSpawner>();
        return groupMap.TryGetValue(group, out var list) 
            ? new List<BossTempWeaponSpawner>(list) 
            : new List<BossTempWeaponSpawner>();
    }
}