using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NodeDestructionPhase : BossPhase, IPuzzleStateProvider
{
    [System.Serializable]
    public class NodeData
    {
        public GameObject nodeObject;
        public BossPillar pillar;
        public FloatActivatorChannel healthChannel;
        public ActivatorID nodeId;        // ADDED: Store the ID separately
        public bool isDestroyed;
    }

    private NodeDestructionPhaseConfig nodeConfig;
    private List<NodeData> activeNodes = new();
    private int nodesDestroyed = 0;
    
    private GameObject currentTempWeapon;
    private TempWeaponPickup currentPickup;
    private int remainingWeaponUses;
    private bool hasWeapon = false;
    private bool waitingForPickup = false;
    
    private readonly Dictionary<ActivatorID, float> floatStates = new();

    public NodeDestructionPhase(Boss.PhaseEntry entry, NodeDestructionPhaseConfig config, Boss boss) 
        : base(entry, boss)
    {
        this.nodeConfig = config;
    }

    protected override void OnPhaseStart()
    {
        SetupNodes();
        StartWavePhase();
    }

    private void SetupNodes()
    {
        foreach (var nodeEntry in nodeConfig.nodes)
        {
            activeNodes.Add(new NodeData
            {
                nodeObject = nodeEntry.nodeObject,
                pillar = nodeEntry.pillar,
                healthChannel = nodeEntry.healthChannel,
                nodeId = nodeEntry.id,              // Store the ID from config
                isDestroyed = false
            });
            
            if (nodeEntry.healthChannel != null)
            {
                nodeEntry.healthChannel.OnStateChanged += OnNodeHealthChanged;
                floatStates[nodeEntry.id] = 1f;
            }
        }
    }

    private void StartWavePhase()
    {
        waitingForPickup = false;
        SpawnWave();
    }

    private void SpawnWave()
    {
        for (int i = 0; i < nodeConfig.enemiesPerWave; i++)
        {
            var pos = GetRandomSpawnPosition();
            boss.SpawnEnemyAt(pos);
        }
    }

    private void OnNodeHealthChanged(ActivatorID id, float value)
    {
        floatStates[id] = value;
        
        // Find node by its stored ID, not by channel property
        var node = activeNodes.FirstOrDefault(n => n.nodeId == id);
        if (node != null && value <= 0 && !node.isDestroyed)
        {
            DestroyNode(node);
        }
    }

    private void DestroyNode(NodeData node)
    {
        node.isDestroyed = true;
        nodesDestroyed++;
        
        node.pillar?.PercentHeight(0.333f);
        entry.onPillarLowered?.Invoke();
        
        if (nodeConfig.nodeDestroyEffect != null)
            Object.Instantiate(nodeConfig.nodeDestroyEffect, node.nodeObject.transform.position, Quaternion.identity);
        
        node.nodeObject.SetActive(false);
        
        if (node.healthChannel != null)
            node.healthChannel.OnStateChanged -= OnNodeHealthChanged;
        
        if (nodesDestroyed >= activeNodes.Count)
        {
            CompletePhase();
        }
        else
        {
            SpawnTemporaryWeapon();
        }
    }

    private void SpawnTemporaryWeapon()
    {
        if (currentPickup != null) return;
        
        var spawnPos = GetRandomSpawnPosition();
        var pickupObj = Object.Instantiate(nodeConfig.tempWeaponPickupPrefab, spawnPos, Quaternion.identity);
        currentPickup = pickupObj.GetComponent<TempWeaponPickup>();
        
        if (currentPickup != null)
        {
            currentPickup.OnPickup += HandleWeaponPickup;
            waitingForPickup = true;
        }
    }

    private void HandleWeaponPickup(TempWeaponPickup pickup)
    {
        hasWeapon = true;
        remainingWeaponUses = nodeConfig.weaponUses;
        waitingForPickup = false;
        
        if (currentPickup != null)
            currentPickup.OnPickup -= HandleWeaponPickup;
        Object.Destroy(pickup.gameObject);
        currentPickup = null;
        
        Debug.Log($"[NodePhase] Picked up temporary weapon! {remainingWeaponUses} uses remaining.");
    }

    public override void Update()
    {
        if (!isActive) return;
        
        if (!waitingForPickup && !hasWeapon && boss.GetEnemiesAliveCount() == 0)
        {
            SpawnTemporaryWeapon();
        }
        
        if (hasWeapon && remainingWeaponUses <= 0)
        {
            hasWeapon = false;
            SpawnWave();
        }
    }

    public bool TryDamageNode(GameObject hitNode)
    {
        if (!hasWeapon) return false;
        
        var node = activeNodes.FirstOrDefault(n => n.nodeObject == hitNode && !n.isDestroyed);
        if (node == null) return false;
        
        remainingWeaponUses--;
        return true;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (nodeConfig.spawnPoints.Length > 0)
        {
            return nodeConfig.spawnPoints[Random.Range(0, nodeConfig.spawnPoints.Length)].position;
        }
        return boss.transform.position + Random.insideUnitSphere * 10f;
    }

    public override void Cleanup()
    {
        if (currentPickup != null)
        {
            currentPickup.OnPickup -= HandleWeaponPickup;
            Object.Destroy(currentPickup.gameObject);
        }
        
        foreach (var node in activeNodes)
        {
            if (node.healthChannel != null)
                node.healthChannel.OnStateChanged -= OnNodeHealthChanged;
        }
        
        base.Cleanup();
    }

    public bool TryGetBool(ActivatorID id, out bool value) { value = default; return false; }
    public bool TryGetFloat(ActivatorID id, out float value) => floatStates.TryGetValue(id, out value);
    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) { value = default; return false; }
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) { value = default; return false; }
    
    protected override bool ShouldLowerPillarOnComplete() => false;
}