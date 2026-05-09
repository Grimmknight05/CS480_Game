/// <summary>
/// Abstract base for boss behavioral states.
/// Each state encapsulates distinct behavior patterns.
/// </summary>
using UnityEngine;
public abstract class BossState
{
    protected Boss boss;
    protected float stateTimer;

    public BossState(Boss boss)
    {
        this.boss = boss;
    }

    public virtual void OnEnter()
    {
        stateTimer = 0f;
    }

    public virtual void Update()
    {
        stateTimer += Time.deltaTime;
    }

    public virtual void OnExit() { }
}