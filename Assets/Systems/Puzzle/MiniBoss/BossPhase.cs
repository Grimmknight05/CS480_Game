using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossPhase
{
    protected Boss.PhaseEntry entry;
    protected Boss boss;
    protected BossPillar pillar;
    protected bool isActive;
    protected ActivatorConfiguration requiredPuzzle;
    protected Dictionary<string, object> stoneStates = new Dictionary<string, object>();

    public event Action OnPhaseComplete;

    public BossPhase(Boss.PhaseEntry entry, Boss boss)
    {
        this.entry = entry;
        this.boss = boss;
        this.requiredPuzzle = entry.puzzleRequirement;
    }

    public void Initialize()
    {
        isActive = true;
        pillar = entry.pillar;
        pillar?.MaxHight();
        entry.onPhaseStart?.Invoke();
        OnPhaseStart();
    }

    // Derived classes can call this to finish the phase.
    // Override ShouldLowerPillarOnComplete() to control pillar behavior.
    protected virtual void CompletePhase()
    {
        if (!isActive) return;
        isActive = false;

        if (ShouldLowerPillarOnComplete())
            pillar?.PercentHeight(0.333f);

        entry.onPhaseComplete?.Invoke();
        OnPhaseComplete?.Invoke();
    }

    // Derived classes can override this to prevent the automatic pillar lowering.
    protected virtual bool ShouldLowerPillarOnComplete() => true;

    protected virtual void CheckPuzzleCompletion() { }

    public virtual void Cleanup() { }
    protected abstract void OnPhaseStart();
    public abstract void Update();
}