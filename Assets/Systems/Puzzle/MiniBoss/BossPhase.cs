using System;

public abstract class BossPhase
{
    protected PhaseConfig config;
    protected Boss boss;
    protected bool isActive;

    public event Action OnPhaseComplete;

    public BossPhase(PhaseConfig config, Boss boss)
    {
        this.config = config;
        this.boss = boss;
    }

    public void Initialize()
    {
        isActive = true;
        config.onPhaseStart?.Invoke();
        OnPhaseStart();
    }

    protected abstract void OnPhaseStart();
    public abstract void Update();

    protected void CompletePhase()
    {
        if (!isActive) return;
        isActive = false;
        config.onPhaseComplete?.Invoke();
        OnPhaseComplete?.Invoke();
    }

    public virtual void Cleanup() { }
}