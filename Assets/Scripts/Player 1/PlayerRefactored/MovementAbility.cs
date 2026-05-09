public abstract class MovementAbility
{
    public bool IsEnabled { get; set; } = true;
    public virtual void Enter(PlayerControllerRefactored player) { }
    public virtual void Exit(PlayerControllerRefactored player) { }
    public virtual bool CanExecute(PlayerControllerRefactored player) => false;
    public virtual void Execute(PlayerControllerRefactored player) { }
}