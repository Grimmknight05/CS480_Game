using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnPhase : BossPhase, IPuzzleStateProvider
{
    private WavePhaseConfig waveConfig;
    private int wavesCompleted;
    private bool waveInProgress;
    private Vector3[] spawnPositions;

    private bool waitingForPuzzle = false;
    private bool pillarLowered = false;
    private FloatActivatorChannel floatChannel;
    private readonly Dictionary<ActivatorID, float> floatStates = new();

    // IPuzzleStateProvider — boss puzzles are stone-rotation (float) only
    public bool TryGetBool(ActivatorID id, out bool value) { value = default; return false; }
    public bool TryGetFloat(ActivatorID id, out float value) => floatStates.TryGetValue(id, out value);
    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) { value = default; return false; }
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) { value = default; return false; }

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
        if (floatChannel != null) return;
        floatChannel = entry.stateChannel;
        if (floatChannel != null)
            floatChannel.OnStateChanged += OnFloatStateChanged;
        else
            Debug.LogError($"Phase '{entry.config.phaseName}' has no FloatActivatorChannel assigned!");
    }

    private void OnFloatStateChanged(ActivatorID id, float value)
    {
        floatStates[id] = value;
        if (waitingForPuzzle)
            CheckPuzzleCompletion();
    }

    protected override void CheckPuzzleCompletion()
    {
        if (requiredPuzzle == null) return;
        if (requiredPuzzle.IsSolved(this))
        {
            waitingForPuzzle = false;
            UnsubscribeFromPuzzleChannel();
            CompletePhase();
        }
    }

    private void UnsubscribeFromPuzzleChannel()
    {
        if (floatChannel != null)
            floatChannel.OnStateChanged -= OnFloatStateChanged;
        floatChannel = null;
    }

    private void SpawnWave()
    {
        for (int i = 0; i < waveConfig.enemiesPerWave; i++)
        {
            var pos = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Length)];
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
