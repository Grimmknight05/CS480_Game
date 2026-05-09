public abstract class MovementState
{
    public virtual void Enter(PlayerControllerRefactored player) { }
    public virtual void Exit(PlayerControllerRefactored player) { }
    public virtual void Tick(PlayerControllerRefactored player) { }
    public virtual void FixedTick(PlayerControllerRefactored player) { }
}