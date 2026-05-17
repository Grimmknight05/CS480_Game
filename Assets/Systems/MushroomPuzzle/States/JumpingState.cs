using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Default critter behavior: small periodic hops, ignoring the player.

public class JumpingState : CritterState
{
    private float nextHopTime;

    public override void Enter(CritterController critter)
    {
        nextHopTime = Time.time + Random.Range(critter.HopIntervalMin, critter.HopIntervalMax);
    }

    public override void Tick(CritterController critter)
    {
        if (Time.time < nextHopTime) return;
        if (critter.Body == null) { nextHopTime = Time.time + critter.HopIntervalMax; return; }

        var impulse = new Vector3(
            Random.Range(-critter.HopHorizontal, critter.HopHorizontal),
            critter.HopVertical,
            Random.Range(-critter.HopHorizontal, critter.HopHorizontal));
        critter.Body.AddForce(impulse, ForceMode.Impulse);
        nextHopTime = Time.time + Random.Range(critter.HopIntervalMin, critter.HopIntervalMax);
    }
}
