using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Boss : MonoBehaviour
{
    [Header("Phases")]
    [SerializeField] protected PhaseConfig[] phaseConfigs;   // assign in inspector

    protected List<BossPhase> phases = new List<BossPhase>();
    protected int currentPhaseIndex = 0;
    protected BossPhase currentPhase;

    protected int health;
    public event Action OnBossDefeated;

    // Enemy tracking (simplified)
    protected List<GameObject> activeEnemies = new List<GameObject>();

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
                case PhaseType.DoorLock:
                    phase = new DoorLockPhase(config, this);
                    break;
                default:
                    Debug.LogWarning($"Unknown phase type {config.phaseType}");
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
        health--;   // reduce health after phase completion
        // Optionally: trigger visual effect etc.
        StartNextPhase();
    }

    protected virtual void Update()
    {
        currentPhase?.Update();
    }

    // Helper methods for phases
    public void SpawnEnemyAt(Vector3 position)
    {
        // Replace with your actual enemy spawning logic (object pool, instantiate...)
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    public int GetEnemiesAliveCount()
    {
        // Remove any destroyed enemies from the list
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
        // Propagate to current phase if needed (for WaveSpawnPhase)
        if (currentPhase is WaveSpawnPhase wavePhase)
            wavePhase.OnEnemyDied();
    }

    protected virtual void DefeatBoss()
    {
        OnBossDefeated?.Invoke();
        // Additional logic: open exit door, play animation, etc.
    }
}