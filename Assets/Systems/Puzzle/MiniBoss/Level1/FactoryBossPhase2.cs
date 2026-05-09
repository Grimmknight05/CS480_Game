using UnityEngine;
public class FactoryBossPhase2 : BossPhase
{
    private PossessableObject[] doorsToLock;
    private float lockTimer;
    private int doorsLocked;

    public FactoryBossPhase2(Boss boss, PhaseData data, ObjectPool<EnemyControllerTest> pool, ActivatorStateChannel stateChannel, BossPillar pillar)
        : base(boss, data, pool)
    {
        doorsToLock = data.doorsToLock;   // assigned in PhaseData asset
    }

    protected override void OnPhaseStart()
    {
        lockTimer = 1f;
        doorsLocked = 0;
    }

    public override void Update()
    {
        if (!isActive) return;
        UpdateChallenges();
        UpdateAttacks();
    }

    protected override void UpdateChallenges()
    {
        if (doorsLocked >= doorsToLock.Length)
        {
            CompleteObjective();
            return;
        }

        lockTimer -= Time.deltaTime;
        if (lockTimer <= 0f)
        {
            LockRandomDoor();
            lockTimer = 2f;
        }
    }

    private void LockRandomDoor()
    {
        if (doorsToLock == null || doorsToLock.Length == 0) return;
        int idx = Random.Range(0, doorsToLock.Length);
        if (!doorsToLock[idx].IsPossessed)
        {
            doorsToLock[idx].OnPossessed();
            doorsLocked++;
        }
    }

    protected override void UpdateAttacks()
    {
        // Spawn occasional enemy to pressure player
        if (Time.time % 4f < 0.05f)
            SpareEnemy();
    }

    private void SpareEnemy()
    {
        var enemy = enemyPool.Get();
        enemy.transform.position = boss.transform.position + Random.insideUnitSphere * 5f;
        enemy.gameObject.SetActive(true);
    }
}