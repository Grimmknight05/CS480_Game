using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeDestructionPhase : BossPhase, IPuzzleStateProvider
{
    [System.Serializable]
    public class NodeData
    {
        public FloatingNode node;
        public BossPillar pillar;
        public int hitCount;
        public bool isDestroyed;
    }

    private NodeDestructionPhaseConfig nodeConfig;
    private Boss.NodeDestructionPhaseEntry phaseEntry;  // specific entry type
    private CentralBoss centralBoss;
    private List<NodeData> activeNodes = new();
    private int nodesDestroyed = 0;
    
    private bool isPuzzleSolved = false;
    private bool isWeaponSpawned = false;
    private bool isShieldDown = false;
    private bool isBossDamagePhase = false;
    private float bossHealthAtPhaseStart;
    private float requiredBossDamage;
    
    private TempWeaponPickup currentWeaponPickup;
    private ToolSystem playerToolSystem;
    
    private readonly Dictionary<ActivatorID, float> floatStates = new();
    
    // Singleton instance for TempNodeWeapon to use
    public static NodeDestructionPhase Instance { get; private set; }

    // Constructor now takes the derived entry
    public NodeDestructionPhase(Boss.NodeDestructionPhaseEntry entry, NodeDestructionPhaseConfig config, Boss boss) 
        : base(entry, boss)
    {
        this.phaseEntry = entry;
        this.nodeConfig = config;
    }

    protected override void OnPhaseStart()
    {
        Instance = this;
        
        // Get scene references from the specific entry
        centralBoss = phaseEntry.centralBossRef;
        if (centralBoss == null)
        {
            Debug.LogError("[NodeDestructionPhase] No CentralBoss assigned in PhaseEntry!");
            return;
        }
        
        SetupNodes();
        SubscribeToStoneChannel();
        
        isPuzzleSolved = false;
        isWeaponSpawned = false;
        isShieldDown = false;
        isBossDamagePhase = false;
        nodesDestroyed = 0;
        bossHealthAtPhaseStart = centralBoss.GetCurrentHealth();
        requiredBossDamage = nodeConfig.BossHealthPerPhase;
        
        // Ensure shield is up at start
        centralBoss.RaiseShield();
        
        // Start enemy waves if configured
        if (nodeConfig.EnemiesPerWave > 0)
            boss.StartCoroutine(SpawnWavesRoutine());
        
        Debug.Log("[NodeDestructionPhase] Phase started. Puzzle awaiting stone rotations.");
    }

    private void SetupNodes()
    {
        var pillars = phaseEntry.nodePillars;
        var nodes = phaseEntry.floatingNodes;
        
        if (pillars == null || nodes == null || pillars.Length != 3 || nodes.Length != 3)
        {
            Debug.LogError("[NodeDestructionPhase] PhaseEntry must have exactly 3 pillars and 3 nodes assigned!");
            return;
        }
        
        activeNodes.Clear();
        for (int i = 0; i < 3; i++)
        {
            activeNodes.Add(new NodeData
            {
                node = nodes[i],
                pillar = pillars[i],
                hitCount = 0,
                isDestroyed = false
            });
        }
    }

    private void SubscribeToStoneChannel()
    {
        if (nodeConfig.StoneStateChannel != null)
        {
            nodeConfig.StoneStateChannel.OnStateChanged += OnStoneStateChanged;
        }
    }

    private void OnStoneStateChanged(ActivatorID id, float value)
    {
        floatStates[id] = value;
    }

    private System.Collections.IEnumerator SpawnWavesRoutine()
    {
        while (!isPuzzleSolved && isActive)
        {
            SpawnWave();
            yield return new WaitForSeconds(nodeConfig.EnemyRespawnDelay);
        }
    }

    private void SpawnWave()
    {
        for (int i = 0; i < nodeConfig.EnemiesPerWave; i++)
        {
            var pos = GetRandomSpawnPosition();
            boss.SpawnEnemyAt(pos);
        }
    }

    public override void Update()
    {
        if (!isActive) return;
        
        // 1. Check puzzle completion
        if (!isPuzzleSolved && nodeConfig.StoneStateChannel != null)
        {
            CheckPuzzleState();
        }
        
        // 2. Spawn weapon when puzzle solved
        if (isPuzzleSolved && !isWeaponSpawned)
        {
            SpawnTemporaryWeapon();
            isWeaponSpawned = true;
        }
        
        // 3. After all nodes destroyed, enter boss damage phase
        if (isPuzzleSolved && nodesDestroyed >= 3 && !isBossDamagePhase)
        {
            EnterBossDamagePhase();
        }
        
        // 4. During boss damage phase, check if enough damage dealt
        if (isBossDamagePhase && isActive)
        {
            float damageDealt = bossHealthAtPhaseStart - centralBoss.GetCurrentHealth();
            if (damageDealt >= requiredBossDamage)
            {
                OnBossDamageThresholdReached();
            }
        }
    }

    private void CheckPuzzleState()
    {
        var stoneReqs = nodeConfig.RequiredStones;
        if (stoneReqs == null || stoneReqs.Length == 0) return;
        
        bool allStoneSatisfied = true;
        foreach (var req in stoneReqs)
        {
            if (req.stoneID == null)
            {
                allStoneSatisfied = false;
                break;
            }
            
            if (!floatStates.TryGetValue(req.stoneID, out float rotation))
            {
                allStoneSatisfied = false;
                break;
            }
            
            if (!req.IsSatisfied(rotation))
            {
                allStoneSatisfied = false;
                break;
            }
        }
        
        if (allStoneSatisfied && !isPuzzleSolved)
        {
            OnPuzzleSolved();
        }
    }

    public void OnPuzzleSolved()
    {
        if (isPuzzleSolved) return;
        isPuzzleSolved = true;
        
        // Lower all pillars to reveal nodes
        foreach (var nodeData in activeNodes)
        {
            nodeData.pillar?.PercentHeight(0f);
        }
        
        Debug.Log("[NodeDestructionPhase] Puzzle solved! Temporary weapon will spawn.");
    }

    private void SpawnTemporaryWeapon()
    {
        if (currentWeaponPickup != null) return;
        
        TempNodeWeapon nodeWeapon = nodeConfig.TempNodeWeapon;
        if (nodeWeapon == null)
        {
            Debug.LogError("[NodeDestructionPhase] No TempNodeWeapon assigned in config!");
            return;
        }
        
        var spawnPos = GetRandomSpawnPosition();
        var pickupObj = new GameObject("TempWeaponPickup");
        pickupObj.transform.position = spawnPos;
        
        var pickup = pickupObj.AddComponent<TempWeaponPickup>();
        pickup.Initialize(nodeWeapon, this);
        pickup.OnPickup += HandleWeaponPickup;
        
        var collider = pickupObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1.5f;
        
        currentWeaponPickup = pickup;
        Debug.Log("[NodeDestructionPhase] Temporary weapon spawned.");
    }

    private void HandleWeaponPickup(TempWeaponPickup pickup)
    {
        Debug.Log("[NodeDestructionPhase] Weapon picked up!");
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerToolSystem = player.GetComponent<ToolSystem>();
        
        // Weapon is now active; player can shoot nodes
    }

    public bool TryDamageNode(GameObject hitNode)
    {
        if (!isPuzzleSolved || currentWeaponPickup == null || currentWeaponPickup.IsCollected == false)
            return false;
        
        var node = activeNodes.FirstOrDefault(n => n.node?.gameObject == hitNode && !n.isDestroyed);
        if (node == null) return false;
        
        node.hitCount++;
        const int hitsToDestroy = 4;
        
        if (node.hitCount >= hitsToDestroy)
        {
            DestroyNode(node);
        }
        
        return true;
    }

    private void DestroyNode(NodeData node)
    {
        node.isDestroyed = true;
        nodesDestroyed++;
        
        // Lower corresponding pillar by cumulative fraction
        if (node.pillar != null)
            node.pillar.PercentHeight(0.333f * nodesDestroyed);
        
        if (nodeConfig.NodeDestroyEffect != null && node.node != null)
            Object.Instantiate(nodeConfig.NodeDestroyEffect, node.node.transform.position, Quaternion.identity);
        
        if (node.node != null)
            node.node.gameObject.SetActive(false);
        
        Debug.Log($"[NodeDestructionPhase] Node destroyed! {nodesDestroyed}/3");
        
        if (nodesDestroyed >= 3)
        {
            RemoveTemporaryWeaponFromPlayer();
        }
    }

    private void RemoveTemporaryWeaponFromPlayer()
    {
        if (playerToolSystem != null)
        {
            playerToolSystem.RemoveTemporaryWeapon();
            playerToolSystem = null;
        }
        
        if (currentWeaponPickup != null)
        {
            currentWeaponPickup.OnPickup -= HandleWeaponPickup;
            Object.Destroy(currentWeaponPickup.gameObject);
            currentWeaponPickup = null;
        }
        
        Debug.Log("[NodeDestructionPhase] Temporary weapon removed.");
    }

    private void EnterBossDamagePhase()
    {
        isBossDamagePhase = true;
        isShieldDown = true;
        centralBoss.LowerShield();
        
        // Record starting health for damage tracking
        bossHealthAtPhaseStart = centralBoss.GetCurrentHealth();
        
        Debug.Log($"[NodeDestructionPhase] Boss damage phase started. Deal {requiredBossDamage} damage.");
    }

    private void OnBossDamageThresholdReached()
    {
        if (!isBossDamagePhase) return;
        
        Debug.Log("[NodeDestructionPhase] Boss damage threshold reached. Completing phase.");
        
        centralBoss.RaiseShield();
        ResetNodesAndPillars();
        CompletePhase();
    }

    private void ResetNodesAndPillars()
    {
        foreach (var nodeData in activeNodes)
        {
            if (nodeData.node != null)
                nodeData.node.gameObject.SetActive(true);
            if (nodeData.pillar != null)
                nodeData.pillar.MaxHeight();  // note: method is MaxHight, not MaxHeight
        }
        nodesDestroyed = 0;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (nodeConfig.SpawnPointGroup != null)
        {
            var allSpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            var matchingPoints = new List<SpawnPoint>();
            foreach (var sp in allSpawnPoints)
            {
                if (sp.SpawnSO == nodeConfig.SpawnPointGroup)
                    matchingPoints.Add(sp);
            }
            if (matchingPoints.Count > 0)
                return matchingPoints[Random.Range(0, matchingPoints.Count)].Position;
        }
        return boss.transform.position + Random.insideUnitSphere * 10f;
    }

    public override void Cleanup()
    {
        if (Instance == this) Instance = null;
        RemoveTemporaryWeaponFromPlayer();
        
        if (nodeConfig.StoneStateChannel != null)
        {
            nodeConfig.StoneStateChannel.OnStateChanged -= OnStoneStateChanged;
        }
        
        base.Cleanup();
    }

    // IPuzzleStateProvider implementation
    public bool TryGetBool(ActivatorID id, out bool value) { value = default; return false; }
    public bool TryGetFloat(ActivatorID id, out float value) => floatStates.TryGetValue(id, out value);
    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) { value = default; return false; }
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) { value = default; return false; }
}