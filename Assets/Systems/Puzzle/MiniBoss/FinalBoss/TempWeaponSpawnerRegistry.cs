using System.Collections.Generic;
using UnityEngine;

public static class TempWeaponSpawnerRegistry
{
    private static Dictionary<SpawnerSO, BossTempWeaponSpawner> registry = new Dictionary<SpawnerSO, BossTempWeaponSpawner>();

    public static void Register(SpawnerSO id, BossTempWeaponSpawner spawner)
    {
        if (id == null) return;
        if (!registry.ContainsKey(id))
            registry.Add(id, spawner);
        else
            Debug.LogWarning($"Duplicate registration for SpawnerSO: {id.name}");
    }

    public static void Unregister(SpawnerSO id)
    {
        if (id != null && registry.ContainsKey(id))
            registry.Remove(id);
    }

    public static BossTempWeaponSpawner GetSpawner(SpawnerSO id)
    {
        if (id == null) return null;
        registry.TryGetValue(id, out var spawner);
        return spawner;
    }
}