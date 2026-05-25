using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BossFinalPhase : BossPhase, IPuzzleStateProvider
{
    private BossFinalPhaseConfig finalConfig;
    private CentralBoss centralBoss;
    private float phaseStartTime;
    private bool bossDead = false;
    
    private readonly Dictionary<ActivatorID, float> floatStates = new();
    
    public BossFinalPhase(Boss.PhaseEntry entry, BossFinalPhaseConfig config, Boss boss) 
        : base(entry, boss)
    {
        this.finalConfig = config;
    }
    
    protected override void OnPhaseStart()
    {
        phaseStartTime = Time.time;
        
        // Find the CentralBoss (should be part of the Boss hierarchy)
        centralBoss = boss.GetComponent<CentralBoss>();
        if (centralBoss == null)
        {
            Debug.LogError("[BossFinalPhase] Could not find CentralBoss component on Boss!");
            return;
        }
        
        // Ensure shield is permanently down
        centralBoss.RaiseShield();
        
        Debug.Log("[BossFinalPhase] Final phase started! Boss is now fully vulnerable.");
    }
    
    public override void Update()
    {
        if (!isActive || centralBoss == null) return;
        
        // Check if boss is dead
        if (centralBoss.GetHealthPercent() <= 0 && !bossDead)
        {
            bossDead = true;
            CompletePhase();
        }
    }
    
    public bool TryGetBool(ActivatorID id, out bool value) { value = default; return false; }
    public bool TryGetFloat(ActivatorID id, out float value) => floatStates.TryGetValue(id, out value);
    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) { value = default; return false; }
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) { value = default; return false; }
    
    protected override bool ShouldLowerPillarOnComplete() => false;
}
