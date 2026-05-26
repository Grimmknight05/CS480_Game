using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeDestructionPhase : BossPhase
{
    private NodeDestructionPhaseConfig nodeConfig;
    private List<GameObject> activeNodes = new List<GameObject>();
    
    private int wavesCompleted;
    private bool waveInProgress;
    private Vector3[] enemySpawnPositions;
    private bool enemiesCleared = false;

    public NodeDestructionPhase(Boss.PhaseEntry entry, NodeDestructionPhaseConfig config, Boss boss)
        : base(entry, boss)
    {
        this.nodeConfig = config;
    }

    protected override void OnPhaseStart()
    {
        wavesCompleted = 0;
        waveInProgress = false;
        enemiesCleared = false;
        activeNodes.Clear();

        SpawnNodes();   // nodes are created and added to activeNodes

        var allSpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        var enemyPoints = allSpawnPoints.Where(sp => sp.SpawnSO == nodeConfig.enemySpawnGroup).ToArray();
        enemySpawnPositions = enemyPoints.Select(sp => sp.Position).ToArray();

        if (enemySpawnPositions.Length == 0)
        {
            Debug.LogWarning($"[NodeDestructionPhase] No enemy spawn points found. Skipping enemies.");
            enemiesCleared = true;
            if (activeNodes.Count == 0)
                CompletePhase();
        }
        else
        {
            StartNextWave();
        }
    }

    private void SpawnNodes()
    {
        var allSpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        var nodePoints = allSpawnPoints.Where(sp => sp.SpawnSO == nodeConfig.nodeSpawnGroup).ToArray();

        if (nodePoints.Length == 0)
        {
            // Fallback: find existing nodes with tag (these are still "spawned" as far as phase is concerned)
            var existingNodes = GameObject.FindGameObjectsWithTag(nodeConfig.nodeTag);
            activeNodes.AddRange(existingNodes);
        }
        else
        {
            foreach (var spawnPoint in nodePoints)
            {
                if (nodeConfig.nodePrefab != null)
                {
                    var node = Object.Instantiate(nodeConfig.nodePrefab);
                    node.transform.SetParent(spawnPoint.transform.parent);
                    node.transform.localPosition = spawnPoint.transform.localPosition;
                    activeNodes.Add(node);
                }
                else
                {
                    Debug.LogError("[NodeDestructionPhase] No nodePrefab assigned in config!");
                }
            }
        }

        if (activeNodes.Count == 0)
            Debug.LogWarning("[NodeDestructionPhase] No nodes spawned – objective may be unclear.");
    }

    private void StartNextWave()
    {
        if (wavesCompleted >= nodeConfig.wavesToSpawn)
            return;

        waveInProgress = true;
        SpawnWave();
    }

    private void SpawnWave()
    {
        for (int i = 0; i < nodeConfig.enemiesPerWave; i++)
        {
            var pos = enemySpawnPositions[Random.Range(0, enemySpawnPositions.Length)];
            boss.SpawnEnemyAt(pos);
        }
    }

    public override void Update()
    {
        if (!isActive) return;

        // Enemy wave handling
        if (!enemiesCleared && enemySpawnPositions.Length > 0)
        {
            if (waveInProgress && boss.GetEnemiesAliveCount() == 0)
            {
                waveInProgress = false;
                wavesCompleted++;

                if (wavesCompleted >= nodeConfig.wavesToSpawn)
                {
                    enemiesCleared = true;
                    pillar?.PercentHeight(0.333f);
                    entry.onPillarLowered?.Invoke();

                    // Disable shield ONLY on nodes that this phase spawned
                    foreach (var node in activeNodes)
                    {
                        if (node != null)
                        {
                            var damageable = node.GetComponent<DamageableObject>();
                            if (damageable != null)
                                damageable.SetShieldActive(false);
                        }
                    }

                    if (activeNodes.Count == 0)
                        CompletePhase();
                }
                else
                {
                    StartNextWave();
                }
            }
        }

        // Node destruction check
        if (enemiesCleared)
        {
            activeNodes.RemoveAll(node => node == null);
            if (activeNodes.Count == 0)
            {
                CompletePhase();
            }
        }
    }

    private void DestroyAllNodes()
    {
        foreach (var node in activeNodes)
            if (node != null) Object.Destroy(node);
        activeNodes.Clear();
    }

    public override void Cleanup()
    {
        activeNodes.Clear();
        base.Cleanup();
    }

    protected override bool ShouldLowerPillarOnComplete() => false;
}