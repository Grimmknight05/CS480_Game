using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class GenericBossPhase : BossPhase
{
    private PhaseConfig config;
    private float spawnTimer;
    private int currentWave;
    private int enemiesKilled;
    private int spawnedEnemies;
    private int doorsLocked;

    // For stone puzzle completion
    private ActivatorStateChannel stateChannel;
    private bool stoneSolved;

    public GenericBossPhase(Boss boss, PhaseConfig config, ObjectPool<EnemyControllerTest> pool,
                            ActivatorStateChannel stateChannel = null)
        : base(boss, pool, config.requiredCompletions)
    {
        this.config = config;
        this.stateChannel = stateChannel;
    }

    public override void Initialize(Boss boss)
    {
        base.Initialize(boss);
        config.onPhaseStart?.Invoke();

        // Subscribe to stone channel if needed
        if (stateChannel != null && config.stoneRequirement != null)
        {
            stateChannel.OnStateChanged += OnStoneRotated;
        }

        OnPhaseStart(); // calls phase-specific start logic
    }

    private void OnStoneRotated(string id, object value)
    {
        if (!stoneSolved && config.stoneRequirement != null && config.stoneRequirement.IsSatisfied(value))
            stoneSolved = true;
    }

    protected override void OnPhaseStart()
    {
        // Reset counters
        currentWave = 0;
        enemiesKilled = 0;
        spawnedEnemies = 0;
        doorsLocked = 0;

        if (config.objectiveType == ObjectiveType.LockDoors)
            spawnTimer = 1f; // initial delay before locking doors
        else
            spawnTimer = config.waveSpawnInterval;
    }

    public override void Update()
    {
        if (!isActive) return;
        UpdateChallenges();
        UpdateAttacks();
    }

    protected override void UpdateChallenges()
    {
        // If stone is required and already solved, complete objective
        if (config.stoneRequirement != null && stoneSolved)
        {
            PhaseComplete();
            return;
        }

        switch (config.objectiveType)
        {
            case ObjectiveType.SpawnWaves:
                UpdateSpawnWaves();
                break;
            case ObjectiveType.LockDoors:
                UpdateLockDoors();
                break;
            case ObjectiveType.KillCount:
                UpdateKillCount();
                break;
            case ObjectiveType.Custom:
                // Do nothing – rely on UnityEvent or external trigger
                break;
        }

        // Check if standard completions reached (waves/doors/kills) and no stone requirement
        if (config.stoneRequirement == null && objectivesCompleted >= config.requiredCompletions)
            PhaseComplete();
    }

    private void UpdateSpawnWaves()
    {
        if (currentWave >= config.requiredCompletions) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnWave();
            spawnTimer = config.waveSpawnInterval;
            currentWave++;
            objectivesCompleted = currentWave;
        }
    }

    private void SpawnWave()
    {
        if (config.spawnPoints.Length == 0) return;
        Transform spawn = config.spawnPoints[currentWave % config.spawnPoints.Length];
        for (int i = 0; i < config.enemiesPerWave; i++)
        {
            var enemy = enemyPool.Get();
            Vector3 offset = UnityEngine.Random.insideUnitSphere * 2f;
            enemy.transform.position = spawn.position + offset;
            enemy.gameObject.SetActive(true);
        }
    }

    private void UpdateLockDoors()
    {
        if (doorsLocked >= config.requiredCompletions) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            LockRandomDoor();
            spawnTimer = 2f;
        }
    }

    private void LockRandomDoor()
    {
        if (config.doorsToLock == null || config.doorsToLock.Length == 0) return;
        int idx = UnityEngine.Random.Range(0, config.doorsToLock.Length);
        if (!config.doorsToLock[idx].IsPossessed)
        {
            config.doorsToLock[idx].OnPossessed();
            doorsLocked++;
            objectivesCompleted = doorsLocked;
        }
    }

    private void UpdateKillCount()
    {
        // We need to track kills via external event. We'll assume enemies call a method.
        // For simplicity, we periodically count living vs spawned.
        var living = Object.FindObjectsByType<EnemyControllerTest>(FindObjectsSortMode.None);
        int livingCount = living.Length;
        enemiesKilled = spawnedEnemies - livingCount;

        if (spawnedEnemies < config.requiredCompletions)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnSingleEnemy();
                spawnTimer = config.enemySpawnInterval;
            }
        }

        if (enemiesKilled >= config.requiredCompletions)
            objectivesCompleted = config.requiredCompletions;
    }

    private void SpawnSingleEnemy()
    {
        var enemy = enemyPool.Get();
        Vector3 pos = boss.transform.position + UnityEngine.Random.insideUnitSphere * 6f;
        pos.y = 0;
        enemy.transform.position = pos;
        enemy.gameObject.SetActive(true);
        spawnedEnemies++;
    }

    protected override void UpdateAttacks()
    {
        if (config.attackPattern == AttackPattern.None) return;
        if (Time.time % config.attackInterval < 0.05f)
        {
            switch (config.attackPattern)
            {
                case AttackPattern.PeriodicLasers:
                    ActivateRandomLaser();
                    break;
                case AttackPattern.RandomEnemySpawn:
                    SpawnSingleEnemy();
                    break;
                case AttackPattern.ActivateAllPossessed:
                    ActivateAllPossessed();
                    break;
            }
        }
    }

    private void ActivateRandomLaser()
    {
        var lasers = Object.FindObjectsByType<PossessableObject>(FindObjectsSortMode.None);
        foreach (var laser in lasers)
            if (laser.IsPossessed) { laser.Activate(); break; }
    }

    private void ActivateAllPossessed()
    {
        var all = Object.FindObjectsByType<PossessableObject>(FindObjectsSortMode.None);
        foreach (var obj in all)
            if (obj.IsPossessed) obj.Activate();
    }

    public override void Cleanup()
    {
        if (stateChannel != null && config.stoneRequirement != null)
            stateChannel.OnStateChanged -= OnStoneRotated;
        base.Cleanup();
        config.onPhaseComplete?.Invoke();
    }
}