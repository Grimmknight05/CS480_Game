// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// State pattern base for critters. Parallel to MushroomState; receiver type differs.

public abstract class CritterState
{
    public virtual void Enter(CritterController critter) { }
    public virtual void Exit(CritterController critter) { }
    public virtual void Tick(CritterController critter) { }
}
