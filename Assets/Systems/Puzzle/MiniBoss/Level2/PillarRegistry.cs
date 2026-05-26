// PillarRegistry.cs
using System.Collections.Generic;
using UnityEngine;

public static class PillarRegistry
{
    private static Dictionary<PillarSO, BossPillar> pillars = new();

    public static void Register(PillarSO id, BossPillar pillar)
    {
        if (id == null || pillar == null) return;
        pillars[id] = pillar;
    }

    public static void Unregister(PillarSO id)
    {
        if (id != null)
            pillars.Remove(id);
    }

    public static BossPillar GetPillar(PillarSO id)
    {
        if (id == null) return null;
        pillars.TryGetValue(id, out var pillar);
        return pillar;
    }
}