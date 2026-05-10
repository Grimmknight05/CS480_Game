using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Boss : MonoBehaviour
{
    [System.Serializable]
    public class PhaseEntry
    {
        public PhaseConfig config;           // can be WavePhaseConfig, DoorLockConfig, etc.
        public BossPillar pillar;            // direct scene reference
        public ActivatorStateChannel stateChannel;
        public UnityEvent onPhaseStart;      // Inspector events that can reference scene objects
        public UnityEvent onPhaseComplete;
        public ActivatorConfiguration puzzleRequirement;
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
    public event Action OnBossStart;
    public UnityEvent OnBossDefeatedEvent;
    public event Action OnBossDefeated;

    protected virtual void Start()
    {
        health = phaseEntries.Count;
        CreatePhases();
    }

    private void CreatePhases()
    {
        foreach (var entry in phaseEntries)
        {
            BossPhase phase = null;
            switch (entry.config.phaseType)
            {
                case PhaseType.WaveSpawn:
                    // entry.config is PhaseConfig, but we need WavePhaseConfig – cast safely
                    if (entry.config is WavePhaseConfig waveConfig)
                        phase = new WaveSpawnPhase(entry, waveConfig, this);
                    else
                        Debug.LogError($"Phase {entry.config.phaseName} is marked WaveSpawn but config is not WavePhaseConfig!");
                    break;
                // other phase types
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

    protected virtual void Update() => currentPhase?.Update();

    public virtual void SpawnEnemyAt(Vector3 position)
    {
        if (enemyPrefab == null) return;
        var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    public int GetEnemiesAliveCount()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }

    protected virtual void DefeatBoss()
    {
        OnBossDefeated?.Invoke();
        OnBossDefeatedEvent?.Invoke();
    } 
}