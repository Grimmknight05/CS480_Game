using UnityEngine;

public class CoreNodeDamagePhase : BossPhase
{
    private CoreNodeDamagePhaseConfig config;
    private DamageableObject coreNode;
    private int requiredHealthThreshold;
    private MeteorSpawner[] meteorSpawners;
    private BossTempWeaponSpawner tempWeaponSpawner;   // cache reference

    public CoreNodeDamagePhase(Boss.PhaseEntry entry, CoreNodeDamagePhaseConfig config, Boss boss)
        : base(entry, boss)
    {
        this.config = config;
    }

    protected override void OnPhaseStart()
    {
        Debug.Log($"[CoreNodeDamagePhase] Phase started: {entry.config.phaseName}");

        if (config == null)
        {
            Debug.LogError("[CoreNodeDamagePhase] Config is null!");
            CompletePhase();
            return;
        }

        // Lock spawner at start (no pickup yet)
        if (config.tempWeaponSpawnerId != null)
        {
            tempWeaponSpawner = TempWeaponSpawnerRegistry.GetSpawner(config.tempWeaponSpawnerId);
            if (tempWeaponSpawner != null)
            {
                tempWeaponSpawner.Unlock();
                Debug.Log($"[CoreNodeDamagePhase] Ensured spawner is unlocked: {config.tempWeaponSpawnerId.name}");
            }
        }

        // Get core node
        if (config.coreNodeIdentifier == null)
        {
            Debug.LogError("[CoreNodeDamagePhase] coreNodeIdentifier is null!");
            CompletePhase();
            return;
        }

        CoreNode coreNodeComponent = CoreNodeRegistry.GetNode(config.coreNodeIdentifier);
        if (coreNodeComponent == null)
        {
            Debug.LogError($"[CoreNodeDamagePhase] No CoreNode found with identifier '{config.coreNodeIdentifier.name}'");
            CompletePhase();
            return;
        }

        coreNode = coreNodeComponent.Damageable;
        if (coreNode == null)
        {
            Debug.LogError($"[CoreNodeDamagePhase] CoreNode '{coreNodeComponent.name}' has no DamageableObject!");
            CompletePhase();
            return;
        }

        requiredHealthThreshold = Mathf.FloorToInt(coreNode.MaxHealth * (1f - config.damageFraction));
        coreNode.SetShieldActive(false);
        coreNode.SetDamageFloor(requiredHealthThreshold);

        // Activate meteor spawners (unchanged)
        if (config.meteorSpawnerGroup != null)
        {
            meteorSpawners = Object.FindObjectsByType<MeteorSpawner>(FindObjectsSortMode.None);
            foreach (var spawner in meteorSpawners)
                if (spawner != null && spawner.spawnerGroup == config.meteorSpawnerGroup)
                    spawner.enabled = true;
        }

        Debug.Log("[CoreNodeDamagePhase] Waiting for player to damage core node.");
    }

    public override void Update()
    {
        if (!isActive) return;

        if (coreNode == null)
        {
            CompletePhase();
            return;
        }

        // When health reaches threshold, unlock spawner and complete
        if (coreNode.CurrentHealth <= requiredHealthThreshold)
        {
            CompletePhase();
        }
    }

    private void UnlockSpawner()
    {
        if (tempWeaponSpawner != null)
        {
            tempWeaponSpawner.Unlock();
            Debug.Log($"[CoreNodeDamagePhase] Unlocked spawner: {config.tempWeaponSpawnerId.name}");
        }
    }

    protected override void CompletePhase()
    {
        // Re-enable shield and clear damage floor
        if (coreNode != null)
        {
            if (coreNode.CurrentHealth > 0)
                coreNode.SetShieldActive(true);
            coreNode.ClearDamageFloor();
        }

        // Deactivate meteor spawners
        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
                if (spawner != null) spawner.enabled = false;
        }

        base.CompletePhase();
    }

    public override void Cleanup()
    {
        // Lock spawner again to destroy any leftover pickup
        if (tempWeaponSpawner != null)
            tempWeaponSpawner.Lock();

        // Deactivate meteor spawners
        if (meteorSpawners != null)
        {
            foreach (var spawner in meteorSpawners)
                if (spawner != null) spawner.enabled = false;
        }

        base.Cleanup();
    }
}