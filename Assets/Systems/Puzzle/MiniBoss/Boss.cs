using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Boss : MonoBehaviour
{
    [Header("Phases")]
    [SerializeField] protected PhaseConfig[] phaseConfigs;   // assign in Inspector

    protected List<BossPhase> phases = new List<BossPhase>();
    protected int currentPhaseIndex = 0;
    protected BossPhase currentPhase;
    protected int health;

    // Enemy tracking
    protected List<GameObject> activeEnemies = new List<GameObject>();

    // Prefab (set in concrete boss)
    protected GameObject enemyPrefab;

    public event Action OnBossDefeated;

    protected virtual void Start()
    {
        health = phaseConfigs.Length;
        CreatePhases();
        StartNextPhase();
    }

    private void CreatePhases()
    {
        foreach (var config in phaseConfigs)
        {
            BossPhase phase = null;
            switch (config.phaseType)
            {
                case PhaseType.WaveSpawn:
                    phase = new WaveSpawnPhase(config, this);
                    break;
                //case PhaseType.DoorLock:
                //    phase = new DoorLockPhase(config, this);
                //    break;
                default:
                    Debug.LogWarning($"Unknown phase type: {config.phaseType}");
                    continue;
            }
            phase.OnPhaseComplete += OnPhaseCompleted;
            phases.Add(phase);
        }
    }

    private void StartNextPhase()
    {
        if (currentPhaseIndex >= phases.Count)
        {
            DefeatBoss();
            return;
        }

        currentPhase = phases[currentPhaseIndex];
        currentPhase.Initialize();
    }

    private void OnPhaseCompleted()
    {
        currentPhase.Cleanup();
        currentPhaseIndex++;
        health--;
        StartNextPhase();
    }

    protected virtual void Update()
    {
        currentPhase?.Update();
    }

    // Helper methods for phases
    public virtual void SpawnEnemyAt(Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not set in concrete boss!");
            return;
        }
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
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
    }
}