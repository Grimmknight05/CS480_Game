using UnityEngine;

public class CoreNodeDamagePhase : BossPhase
{
    private CoreNodeDamagePhaseConfig config;
    private DamageableObject coreNode;
    private int requiredHealthThreshold;
    private MeteorSpawner[] meteorSpawners;

    public CoreNodeDamagePhase(Boss.PhaseEntry entry, CoreNodeDamagePhaseConfig config, Boss boss)
        : base(entry, boss)
    {
        this.config = config;
    }

    protected override void OnPhaseStart()
    {
        // Get core node from registry using identifier
        Debug.Log($"[CoreNodeDamagePhase] Phase started: {entry.config.phaseName}");
                // 1. Validate config
        if (config == null)
        {
            Debug.LogError("[CoreNodeDamagePhase] Config is null!");
            CompletePhase();
            return;
        }

        // 2. Get core node from registry
        if (config.coreNodeIdentifier == null)
        {
            Debug.LogError("[CoreNodeDamagePhase] coreNodeIdentifier is null in config!");
            CompletePhase();
            return;
        }

        Debug.Log($"[CoreNodeDamagePhase] Looking for core node with identifier: {config.coreNodeIdentifier.name}");

        CoreNode coreNodeComponent = CoreNodeRegistry.GetNode(config.coreNodeIdentifier);
        if (coreNodeComponent == null)
        {
            Debug.LogError($"[CoreNodeDamagePhase] No CoreNode registered with identifier '{config.coreNodeIdentifier.name}'. Make sure the CoreNode component in the scene has this identifier assigned and is active.");
            CompletePhase();
            return;
        }
        Debug.Log($"[CoreNodeDamagePhase] Found CoreNode: {coreNodeComponent.name}");
        coreNode = coreNodeComponent.Damageable;
        if (coreNode == null)
        {
            Debug.LogError($"[CoreNodeDamagePhase] CoreNode '{coreNodeComponent.name}' has no DamageableObject component!");
            CompletePhase();
            return;
        }
        Debug.Log($"[CoreNodeDamagePhase] Core node max health: {coreNode.MaxHealth}, current health: {coreNode.CurrentHealth}");
        requiredHealthThreshold = Mathf.FloorToInt(coreNode.MaxHealth * (1f - config.damageFraction));
        Debug.Log($"[CoreNodeDamagePhase] Damage fraction: {config.damageFraction}, threshold health: {requiredHealthThreshold} (health must be ≤ {requiredHealthThreshold} to complete)");
        Debug.Log($"[CoreNodeDamagePhase] Removing shield on core node");
        coreNode.SetShieldActive(false);
         Debug.Log($"[CoreNodeDamagePhase] Shield active status: {coreNode.IsShielded}");
        coreNode.SetDamageFloor(requiredHealthThreshold);
        // Activate meteor spawners by group
        if (config.meteorSpawnerGroup != null)
        {
            Debug.Log($"[CoreNodeDamagePhase] Activating meteor spawners with group: {config.meteorSpawnerGroup.name}");
            meteorSpawners = Object.FindObjectsByType<MeteorSpawner>(FindObjectsSortMode.None);
            int activatedCount = 0;
            foreach (var spawner in meteorSpawners)
            {
                if (spawner != null && spawner.spawnerGroup == config.meteorSpawnerGroup)
                {
                    spawner.enabled = true;
                    activatedCount++;
                    Debug.Log($"[CoreNodeDamagePhase] Activated MeteorSpawner: {spawner.name}");
                }
            }
            Debug.Log($"[CoreNodeDamagePhase] Activated {activatedCount} meteor spawners out of {meteorSpawners.Length} total found.");
        }
        else
        {
            Debug.LogWarning("[CoreNodeDamagePhase] meteorSpawnerGroup is null in config – no meteor spawners will be activated.");
        }
        Debug.Log("[CoreNodeDamagePhase] Initialization complete. Waiting for player to damage core node.");
    }

    public override void Update()
    {
        if (!isActive) return;

        if (coreNode == null)
        {
            CompletePhase();
            return;
        }

        // When health reaches exactly the floor (or below), complete phase
        if (coreNode.CurrentHealth <= requiredHealthThreshold)
        {
            CompletePhase();
        }
    }
    protected override void CompletePhase()
    {
        // Re-enable shield and clear damage floor before completing
        if (coreNode != null)
        {
            // Only re-enable shield if the node is not dead
            if (coreNode.CurrentHealth > 0)
            {
                coreNode.SetShieldActive(true);
            }
            coreNode.ClearDamageFloor();
        }
        // Deactivate meteor spawners (existing cleanup)
        // Then call base CompletePhase
        base.CompletePhase();
    }

    public override void Cleanup()
    {
        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
                if (spawner != null) spawner.enabled = false;
        }
        base.Cleanup();
    }
}