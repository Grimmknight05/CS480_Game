using UnityEngine;

public class WaveSpawnPhase : BossPhase
{
    private int wavesCompleted;
    private int currentWaveEnemiesAlive;
    private float spawnTimer;
    private bool waveInProgress;

    public WaveSpawnPhase(PhaseConfig config, Boss boss) : base(config, boss) { }

    protected override void OnPhaseStart()
    {
        wavesCompleted = 0;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (wavesCompleted >= config.wavesToSpawn)
        {
            CompletePhase();
            return;
        }

        waveInProgress = true;
        currentWaveEnemiesAlive = config.enemiesPerWave;
        spawnTimer = 0f;
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (config.spawnPoints.Length == 0) return;
        for (int i = 0; i < config.enemiesPerWave; i++)
        {
            Transform spawnPoint = config.spawnPoints[Random.Range(0, config.spawnPoints.Length)];
            // Use your enemy spawning logic (object pool, instantiate, etc.)
            // For now, just log or call a boss method to spawn
            boss.SpawnEnemyAt(spawnPoint.position);
        }
    }

    public override void Update()
    {
        if (!isActive) return;

        if (waveInProgress)
        {
            // Check if all enemies in current wave are dead.
            // This assumes boss keeps track of alive enemies.
            if (boss.GetEnemiesAliveCount() == 0)
            {
                waveInProgress = false;
                wavesCompleted++;
                StartNextWave();
            }
        }
    }

    public void OnEnemyDied()
    {
        // Called by the boss when an enemy is killed
        currentWaveEnemiesAlive--;
    }
}