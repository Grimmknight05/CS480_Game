// CoreNodeRegistry.cs
using System.Collections.Generic;
using UnityEngine;

public static class CoreNodeRegistry
{
    private static Dictionary<CoreNodeIdentifierSO, CoreNode> nodes = new();

    public static void Register(CoreNodeIdentifierSO id, CoreNode node)
    {
        if (id == null || node == null) return;
        nodes[id] = node;
    }

    public static void Unregister(CoreNodeIdentifierSO id)
    {
        if (id != null) nodes.Remove(id);
    }

    public static CoreNode GetNode(CoreNodeIdentifierSO id)
    {
        nodes.TryGetValue(id, out var node);
        return node;
    }
}