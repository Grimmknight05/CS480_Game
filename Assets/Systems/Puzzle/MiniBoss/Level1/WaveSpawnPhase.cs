using UnityEngine;

public class WaveSpawnPhase : BossPhase
{
    private int wavesCompleted;
    private bool waveInProgress;

    public WaveSpawnPhase(PhaseConfig config, Boss boss) : base(config, boss) { }

    protected override void OnPhaseStart() => StartNextWave();

    private void StartNextWave()
    {
        if (wavesCompleted >= config.wavesToSpawn)
        {
            CompletePhase();
            return;
        }
        waveInProgress = true;
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (config.spawnPoints.Length == 0) return;
        for (int i = 0; i < config.enemiesPerWave; i++)
        {
            var pos = config.spawnPoints[Random.Range(0, config.spawnPoints.Length)].position;
            boss.SpawnEnemyAt(pos);
        }
    }

    public override void Update()
    {
        if (!isActive) return;
        if (waveInProgress && boss.GetEnemiesAliveCount() == 0)
        {
            waveInProgress = false;
            wavesCompleted++;
            StartNextWave();
        }
    }
}