using UnityEngine;
public class FactoryBossPhase3 : BossPhase
{
    private float spawnTimer;
    private int enemiesSpawned;
    private int enemiesKilled;

    public FactoryBossPhase3(Boss boss, PhaseData data, ObjectPool<EnemyControllerTest> pool,
                             ActivatorStateChannel stateChannel, BossPillar pillar)
        : base(boss, data, pool) { }

    protected override void OnPhaseStart()
    {
        spawnTimer = data.spawnInterval;
        enemiesSpawned = 0;
        enemiesKilled = 0;
    }

    public override void Update()
    {
        if (!isActive) return;
        UpdateChallenges();
        UpdateAttacks();
    }

    protected override void UpdateChallenges()
    {
        // Spawn enemies until maxWaves reached (maxWaves = total enemies)
        if (enemiesSpawned < data.maxWaves)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = data.spawnInterval;
            }
        }

        // Check if all spawned enemies are dead
        var living = Object.FindObjectsByType<EnemyControllerTest>(FindObjectsSortMode.None);
        if (enemiesSpawned >= data.maxWaves && living.Length == 0 && enemiesKilled == enemiesSpawned)
            CompleteObjective();
    }

    private void SpawnEnemy()
    {
        Vector3 pos = boss.transform.position + Random.insideUnitSphere * 6f;
        pos.y = 0;
        var enemy = enemyPool.Get();
        enemy.transform.position = pos;
        enemy.gameObject.SetActive(true);
        enemiesSpawned++;
    }

    // Call this from enemy's OnDeath event (you'll need to wire this up)
    public void OnEnemyKilled()
    {
        enemiesKilled++;
    }

    protected override void UpdateAttacks()
    {
        // Activate all possessed objects periodically
        if (Time.time % 7f < 0.05f)
        {
            var allPossessed = Object.FindObjectsByType<PossessableObject>(FindObjectsSortMode.None);
            foreach (var obj in allPossessed)
                if (obj.IsPossessed) obj.Activate();
        }
    }
}