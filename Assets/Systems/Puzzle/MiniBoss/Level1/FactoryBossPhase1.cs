using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
public class FactoryBossPhase1 : BossPhase
{
    private float spawnTimer;
    private int currentWave;

    public FactoryBossPhase1(Boss boss, PhaseData data, ObjectPool<EnemyControllerTest> pool, ActivatorStateChannel stateChannel, BossPillar pillar)
        : base(boss, data, pool) { }

    protected override void OnPhaseStart()
    {
        spawnTimer = data.spawnInterval;
        currentWave = 0;
    }

    public override void Update()
    {
        if (!isActive) return;
        UpdateChallenges();
        UpdateAttacks();
    }

    protected override void UpdateChallenges()
    {
        if (currentWave >= data.maxWaves) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnWave();
            spawnTimer = data.spawnInterval;
        }

        // Check if all enemies are dead
        var enemies = UnityEngine.Object.FindObjectsByType<EnemyControllerTest>(FindObjectsSortMode.None);
        if (enemies.Length == 0 && currentWave >= data.maxWaves)
            CompleteObjective();
    }

    private void SpawnWave()
    {
        if (data.spawnPoints.Length == 0) return;
        Vector3 spawnPos = data.spawnPoints[currentWave % data.spawnPoints.Length];
        for (int i = 0; i < data.enemiesPerWave; i++)
        {
            var enemy = enemyPool.Get();
            enemy.transform.position = spawnPos + UnityEngine.Random.insideUnitSphere * 2f;
            enemy.gameObject.SetActive(true);
        }
        currentWave++;
    }

    protected override void UpdateAttacks()
    {
        if (!data.enableLaserAttacks) return;
        if (Time.time % data.laserAttackInterval < 0.05f)
            ActivateRandomLaser();
    }

    private void ActivateRandomLaser()
    {
        var lasers = UnityEngine.Object.FindObjectsByType<PossessableObject>(FindObjectsSortMode.None);
        foreach (var laser in lasers)
            if (laser.IsPossessed) { laser.Activate(); break; }
    }
}