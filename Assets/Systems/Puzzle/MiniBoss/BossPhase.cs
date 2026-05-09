using System;

public abstract class BossPhase
{
    protected Boss boss;
    protected ObjectPool<EnemyControllerTest> enemyPool;
    protected int objectivesRequired;
    protected int objectivesCompleted;
    protected bool isActive;

    public event Action OnPhaseComplete;

    protected BossPhase(Boss boss, ObjectPool<EnemyControllerTest> pool, int required)
    {
        this.boss = boss;
        this.enemyPool = pool;
        this.objectivesRequired = required;
    }

    public virtual void Initialize(Boss boss)
    {
        this.boss = boss;
        isActive = true;
        objectivesCompleted = 0;
    }

    protected abstract void OnPhaseStart();
    public abstract void Update();
    protected abstract void UpdateChallenges();
    protected abstract void UpdateAttacks();

    protected virtual void PhaseComplete()
    {
        if (!isActive) return;
        isActive = false;
        OnPhaseComplete?.Invoke();
    }

    public virtual void Cleanup() { }
}