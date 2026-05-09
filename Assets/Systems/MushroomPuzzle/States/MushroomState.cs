// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// State pattern base. Mirrors MovementState shape (Enter/Exit/Tick) for consistency
// with Josh's player-side state machine.

public abstract class MushroomState
{
    public virtual void Enter(Mushroom mushroom) { }
    public virtual void Exit(Mushroom mushroom) { }
    public virtual void Tick(Mushroom mushroom) { }
}
