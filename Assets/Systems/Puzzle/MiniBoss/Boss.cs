using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Boss : ResettableBehaviour
{
    [System.Serializable]
    public class PhaseEntry
    {
        public PhaseConfig config;           // can be WavePhaseConfig, DoorLockConfig, etc.
        public BossPillar pillar;            // direct scene reference
        public FloatActivatorChannel stateChannel;
        public UnityEvent onPhaseStart;      // Inspector events that can reference scene objects
        public UnityEvent onPhaseComplete;
        public ActivatorConfiguration puzzleRequirement;
        public UnityEvent onPillarLowered;
    }
    
    [Header("Phases")]
    [SerializeField] protected List<PhaseEntry> phaseEntries;

    protected List<BossPhase> phases = new List<BossPhase>();
    protected int currentPhaseIndex = 0;
    protected BossPhase currentPhase;
    protected int health;
    protected bool isFightActive = false;

    protected List<GameObject> activeEnemies = new List<GameObject>();
    protected GameObject enemyPrefab;
    public UnityEvent OnBossStartEvent;
    //public event Action OnBossStart;
    public UnityEvent OnBossDefeatedEvent;
    public event Action OnBossDefeated;
    [SerializeField] private EnemyDeathChannel deathChannel;
    private int initialPhaseIndex;
    private bool initialFightActive;
    protected virtual void Start()
    {
        initialPhaseIndex = currentPhaseIndex;
        initialFightActive = isFightActive;
        health = phaseEntries.Count;
        CreatePhases();
    }
    protected override void OnEnable()
    {
        base.OnEnable();//subscribes to resetChannel
        if (deathChannel != null)
            deathChannel.OnEnemyDied += HandleEnemyDied;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (deathChannel != null)
            deathChannel.OnEnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied(GameObject deadEnemy)
    {
        activeEnemies.Remove(deadEnemy);
    }
    private void CreatePhases()
    {
        foreach (var entry in phaseEntries)
        {
            BossPhase phase = null;
            switch (entry.config.phaseType)
            {
                case PhaseType.WaveSpawn:
                    if (entry.config is WavePhaseConfig waveConfig)
                        phase = new WaveSpawnPhase(entry, waveConfig, this);
                    break;
                    
                case PhaseType.NodeDestruction:
                    if (entry.config is NodeDestructionPhaseConfig nodeConfig)
                    {
                        // Cast entry to NodeDestructionPhaseEntry
                        var nodeEntry = entry as NodeDestructionPhaseEntry;
                        if (nodeEntry == null)
                        {
                            Debug.LogError($"Phase {entry.config.phaseName} is NodeDestruction but entry is not NodeDestructionPhaseEntry!");
                            break;
                        }
                        phase = new NodeDestructionPhase(nodeEntry, nodeConfig, this);
                    }
                    break;
                    
                case PhaseType.FinalBoss:
                    if (entry.config is BossFinalPhaseConfig finalConfig)
                        phase = new BossFinalPhase(entry, finalConfig, this);
                    break;
            }
            if (phase != null)
            {
                phase.OnPhaseComplete += OnPhaseCompleted;
                phases.Add(phase);
            }
        }
    }

    public void BeginFight()
    {
        if (isFightActive) return;
        isFightActive = true;
        OnBossStartEvent?.Invoke();
        StartNextPhase();
    }

    protected virtual void StartNextPhase()
    {
        if (currentPhaseIndex >= phases.Count)
        {
            DefeatBoss();
            return;
        }
        currentPhase = phases[currentPhaseIndex];
        currentPhase.Initialize();
    }

    protected virtual void OnPhaseCompleted()
    {
        currentPhase.Cleanup();
        currentPhaseIndex++;
        health--;
        StartNextPhase();
    }

    protected virtual void Update()
    {
        if (isFightActive && currentPhase != null)
            currentPhase.Update();
    }

    public virtual void SpawnEnemyAt(Vector3 position)
    {
        if (!isFightActive)
        {
            Debug.Log($"[Boss] {name} tried to spawn enemy but fight is not active. Ignoring.");
            return;
        }
        if (enemyPrefab == null) return;
        var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    public int GetEnemiesAliveCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        int alive = 0;
        foreach (var e in activeEnemies)
        {
            var ctrl = e.GetComponent<EnemyControllerTest>();
            if (ctrl == null || !ctrl.IsDead) alive++;
        }
        return alive;
    }

    protected virtual void DefeatBoss()
    {
        OnBossDefeated?.Invoke();
        OnBossDefeatedEvent?.Invoke();
    } 

    // ----- ResettableBehaviour implementation -----
    protected override void ResetInternal()
    {
        // If the area this boss belongs to is already completed, do nothing
        // (base class already checks area completion before calling ResetInternal)

        // Stop current phase if active
        if (currentPhase != null)
        {
            currentPhase.Cleanup();
            currentPhase = null;
        }

        // Destroy all active enemies
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();

        // Reset state variables
        currentPhaseIndex = initialPhaseIndex;
        isFightActive = initialFightActive;
        health = phaseEntries.Count;

        // Reset any other boss-specific state (e.g., custom flags)
        // Optionally re‑create phases if they were modified, but usually not needed.

        // If the boss has an OnBossDefeated event, we don't want it to fire on reset.
        // Just return to idle.

        Debug.Log($"[Boss] {name} reset to initial state.");
    }
}