// ============================================
// BOSS STATE: IDLE
// ============================================
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class FactoryBossIdleState : BossState
{
    public FactoryBossIdleState(Boss boss) : base(boss) { }

    public override void OnEnter()
    {
        base.OnEnter();
        // Boss is waiting for player to enter arena
    }

    public override void Update()
    {
        // Idle state doesn't do anything
    }
}

// ============================================
// BOSS STATE: ACTIVE
// ============================================

public class FactoryBossActiveState : BossState
{
    public FactoryBossActiveState(Boss boss) : base(boss) { }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
    }
}

// ============================================
// BOSS STATE: PHASE TRANSITION
// ============================================

public class FactoryBossPhaseTransitionState : BossState
{
    private float transitionDuration;

    public FactoryBossPhaseTransitionState(Boss boss, float duration) : base(boss)
    {
        transitionDuration = duration;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;
    }
}

// ============================================
// BOSS STATE: DEFEATED
// ============================================

public class FactoryBossDefeatedState : BossState
{
    public FactoryBossDefeatedState(Boss boss) : base(boss) { }

    public override void OnEnter()
    {
        base.OnEnter();
        // Play defeat animation, play sound, etc.
    }

    public override void Update()
    {
        // Boss is dead, do nothing
    }
}
