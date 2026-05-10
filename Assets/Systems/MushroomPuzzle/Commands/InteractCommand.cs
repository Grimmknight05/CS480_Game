using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Concrete command produced when the player enters a mushroom's trigger.
// Buffer window matches Josh's defaultCommandLifetime so input feel stays consistent.

public class InteractCommand : IPuzzleCommand
{
    private readonly float timeIssued;
    private readonly float expiryDelay;

    public InteractCommand(float expiryDelay = 0.2f)
    {
        this.timeIssued = Time.time;
        this.expiryDelay = expiryDelay;
    }

    public float ExpiryTime => timeIssued + expiryDelay;

    public bool CanExecute(Mushroom mushroom)
    {
        if (mushroom == null) return false;
        if (!mushroom.IsActive) return true;
        return mushroom.AllowRepeatActivationWhileActive;
    }

    public void Execute(Mushroom mushroom)
    {
        mushroom.Activate();
    }

    public void Undo(Mushroom mushroom)
    {
        mushroom.SetState(new DormantState());
    }
}
