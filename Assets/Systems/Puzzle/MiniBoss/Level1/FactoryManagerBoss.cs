using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryManagerBoss : Boss
{
    [Header("Arena Doors")]
    [SerializeField] private DoorLerp entranceDoor;
    [SerializeField] private DoorLerp exitDoor;

    [Header("Pillars")]
    [SerializeField] private BossPillar[] pillars = new BossPillar[3];

    [Header("Phase Configs (ScriptableObjects)")]
    [SerializeField] private PhaseConfig[] phaseConfigs; // Order = phase order

    [Header("Enemy Pool")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 20;

    [Header("Stone Channel")]
    [SerializeField] private ActivatorStateChannel stateChannel;

    private ObjectPool<EnemyControllerTest> enemyPool;
    private List<GenericBossPhase> activePhases = new List<GenericBossPhase>();
    private int currentPhaseIndex = 0;
    private int phasesCompleted = 0;

    protected override void Awake()
    {
        base.Awake();
        enemyPool = new ObjectPool<EnemyControllerTest>(
            enemyPrefab.GetComponent<EnemyControllerTest>(),
            poolSize
        );
    }

    public void StartBossEncounter()
    {
        if (currentPhaseIndex >= phaseConfigs.Length) return;
        entranceDoor?.Close();
        foreach (var p in pillars) p.MaxHight();
        StartNextPhase();
    }

    private void StartNextPhase()
    {
        if (currentPhaseIndex >= phaseConfigs.Length)
        {
            OnAllPhasesComplete();
            return;
        }

        PhaseConfig cfg = phaseConfigs[currentPhaseIndex];
        var phase = new GenericBossPhase(this, cfg, enemyPool, stateChannel);
        activePhases.Add(phase);
        phase.OnPhaseComplete += () => OnPhaseCompleted(phase, currentPhaseIndex);
        phase.Initialize(this);
    }

    private void OnPhaseCompleted(GenericBossPhase phase, int index)
    {
        activePhases.Remove(phase);
        phase.Cleanup();

        if (index < pillars.Length)
            pillars[index].PercentHeight(0.333f);

        phasesCompleted++;
        currentPhaseIndex++;
        StartNextPhase();
    }

    private void OnAllPhasesComplete()
    {
        exitDoor?.Open();
        DefeatBoss(); // triggers OnBossDefeated event
    }

    protected override void Update()
    {
        base.Update();
        foreach (var p in activePhases) p.Update();
    }

    // Required overrides
    protected override void InitializeStates() { /* ... existing states ... */ }
    protected override void InitializePhases() { }
    protected override Bounds GetLevelBounds() => new Bounds(Vector3.zero, Vector3.one * 50f);
    public override void PossessObject(IPossessable target) => target.OnPossessed();
    public override void ReleasePossession(IPossessable target) => target.OnReleasePossession();
}