using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Critter rests under an active mushroom's cap. Returns to Jumping when the
// timer (matching the mushroom's active duration) expires.

public class CalmUnderCapState : CritterState
{
    private readonly float duration;
    private float endTime;

    public CalmUnderCapState(float duration)
    {
        this.duration = duration;
    }

    public override void Enter(CritterController critter)
    {
        endTime = Time.time + duration;
        if (critter.Body != null)
        {
            critter.Body.linearVelocity = Vector3.zero;
            critter.Body.angularVelocity = Vector3.zero;
        }
    }

    public override void Tick(CritterController critter)
    {
        if (Time.time >= endTime)
        {
            critter.SetState(new JumpingState());
        }
    }
}
