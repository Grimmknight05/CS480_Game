using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeDestructionPhase : BossPhase
{
    private NodeDestructionPhaseConfig nodeConfig;
    private List<GameObject> activeNodes = new List<GameObject>();

    public NodeDestructionPhase(Boss.PhaseEntry entry, NodeDestructionPhaseConfig config, Boss boss)
        : base(entry, boss)
    {
        this.nodeConfig = config;
    }

    protected override void OnPhaseStart()
    {
        // Clear any previous nodes (safety)
        DestroyAllNodes();

        // Find all spawn points for this group
        var allSpawnPoints = UnityEngine.Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        var mySpawnPoints = allSpawnPoints.Where(sp => sp.SpawnSO == nodeConfig.nodeSpawnGroup).ToArray();

        if (mySpawnPoints.Length == 0)
        {
            Debug.LogWarning($"[NodeDestructionPhase] No spawn points found for group {nodeConfig.nodeSpawnGroup}. Falling back to static tag search.");
            // Fallback: find existing nodes with the tag
            var existingNodes = GameObject.FindGameObjectsWithTag(nodeConfig.nodeTag);
            activeNodes.AddRange(existingNodes);
        }
        else
        {
            // Spawn a node at each spawn point position
            foreach (var spawnPoint in mySpawnPoints)
            {
                if (nodeConfig.nodePrefab != null)
                {
                    var node = Object.Instantiate(nodeConfig.nodePrefab, spawnPoint.Position, Quaternion.identity);
                    activeNodes.Add(node);
                }
                else
                {
                    Debug.LogError("[NodeDestructionPhase] No nodePrefab assigned in config!");
                }
            }
        }

        if (activeNodes.Count == 0)
        {
            Debug.LogWarning("[NodeDestructionPhase] No nodes to destroy – completing immediately.");
            CompletePhase();
        }
    }

    public override void Update()
    {
        if (!isActive) return;
        activeNodes.RemoveAll(node => node == null);
        if (activeNodes.Count == 0)
        {
            CompletePhase();
        }
    }

    private void DestroyAllNodes()
    {
        foreach (var node in activeNodes)
        {
            if (node != null) Object.Destroy(node);
        }
        activeNodes.Clear();
    }

    public override void Cleanup()
    {
        // Do not destroy nodes here – they remain destroyed after phase ends
        // But we clear the list to avoid reference leaks
        activeNodes.Clear();
        base.Cleanup();
    }

    protected override bool ShouldLowerPillarOnComplete() => true;
}