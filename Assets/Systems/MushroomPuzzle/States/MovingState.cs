using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Walking toward an active mushroom's cap. Transitions to CalmUnderCapState on arrival.

public class MovingState : CritterState
{
    private readonly Vector3 destination;
    private readonly float arriveRadius;
    private readonly float walkBudget;
    private readonly float calmDuration;
    private readonly bool calmPermanent;
    private float endTime;

    public MovingState(Vector3 destination, float arriveRadius, float walkBudget, float calmDuration, bool calmPermanent = false)
    {
        this.destination = destination;
        this.arriveRadius = arriveRadius;
        this.walkBudget = walkBudget;
        this.calmDuration = calmDuration;
        this.calmPermanent = calmPermanent;
    }

    public override void Enter(CritterController critter)
    {
        endTime = Time.time + walkBudget;
    }

    public override void Tick(CritterController critter)
    {
        var here = critter.transform.position;
        var toTarget = destination - here;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= arriveRadius * arriveRadius || Time.time > endTime)
        {
            critter.SetState(new CalmUnderCapState(calmDuration, calmPermanent));
            return;
        }

        var step = toTarget.normalized * critter.WalkSpeed * Time.deltaTime;
        critter.transform.position = here + step;
    }
}
