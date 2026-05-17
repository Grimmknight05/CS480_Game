using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class WaveSpawnPhase : BossPhase
{
    private WavePhaseConfig waveConfig;
    private int wavesCompleted;
    private bool waveInProgress;
    private Vector3[] spawnPositions;

    private bool waitingForPuzzle = false;
    private bool pillarLowered = false;
    private ActivatorStateChannel stateChannel;

    public WaveSpawnPhase(Boss.PhaseEntry entry, WavePhaseConfig waveConfig, Boss boss)
        : base(entry, boss)
    {
        this.waveConfig = waveConfig;
    }

    protected override void OnPhaseStart()
    {
        var all = UnityEngine.Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        var matching = all.Where(sp => sp.Group == waveConfig.spawnGroupName).ToArray();
        spawnPositions = matching.Select(sp => sp.Position).ToArray();
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (wavesCompleted >= waveConfig.wavesToSpawn)
        {
            if (!pillarLowered && pillar != null)
            {
                pillar.PercentHeight(0.333f);
                pillarLowered = true;
                entry.onPillarLowered?.Invoke();
            }

            if (requiredPuzzle != null)
            {
                waitingForPuzzle = true;
                SubscribeToPuzzleChannel();
                CheckPuzzleCompletion();
            }
            else
            {
                CompletePhase();
            }
            return;
        }

        if (spawnPositions.Length == 0)
        {
            CompletePhase();
            return;
        }

        waveInProgress = true;
        SpawnWave();
    }

    private void SubscribeToPuzzleChannel()
    {
        if (stateChannel != null) return;
        stateChannel = entry.stateChannel;
        if (stateChannel != null)
            stateChannel.OnStateChanged += OnStoneStateChanged;
        else
            Debug.LogError($"Phase '{entry.config.phaseName}' has no ActivatorStateChannel assigned!");
    }

    private void OnStoneStateChanged(string activatorID, object state)
    {
        stoneStates[activatorID] = state;
        if (waitingForPuzzle)
            CheckPuzzleCompletion();
    }

    protected override void CheckPuzzleCompletion()
    {
        if (requiredPuzzle == null) return;

        bool allSatisfied = true;
        foreach (var req in requiredPuzzle.GetRequirements())
        {
            if (!stoneStates.ContainsKey(req.ActivatorID))
            {
                allSatisfied = false;
                break;
            }
            if (!req.IsSatisfied(stoneStates[req.ActivatorID]))
            {
                allSatisfied = false;
                break;
            }
        }

        if (allSatisfied)
        {
            waitingForPuzzle = false;
            UnsubscribeFromPuzzleChannel();
            CompletePhase();
        }
    }

    private void UnsubscribeFromPuzzleChannel()
    {
        if (stateChannel != null)
            stateChannel.OnStateChanged -= OnStoneStateChanged;
        stateChannel = null;
    }

    private void SpawnWave()
    {
        for (int i = 0; i < waveConfig.enemiesPerWave; i++)
        {
            var pos = spawnPositions[Random.Range(0, spawnPositions.Length)];
            boss.SpawnEnemyAt(pos);
        }
    }

    public override void Update()
    {
        if (!isActive) return;
        if (!waitingForPuzzle && waveInProgress && boss.GetEnemiesAliveCount() == 0)
        {
            waveInProgress = false;
            wavesCompleted++;
            StartNextWave();
        }
    }

    public override void Cleanup()
    {
        UnsubscribeFromPuzzleChannel();
        base.Cleanup();
    }

    protected override bool ShouldLowerPillarOnComplete() => false;
}