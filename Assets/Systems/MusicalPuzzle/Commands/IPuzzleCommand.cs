// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Command pattern, parallel to Josh's ICommand. Receiver is Mushroom rather than
// PlayerControllerRefactored so the two queues stay independent.

public interface IPuzzleCommand
{
    float ExpiryTime { get; }
    bool CanExecute(Mushroom mushroom);
    void Execute(Mushroom mushroom);
    void Undo(Mushroom mushroom);
}
