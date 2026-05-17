
// Interface for any requirement that checks if an activator satisfies a condition.
// This allows different activator types (stones, levers, buttons) to have different requirement logic.

public interface IActivatorRequirement<T>
{
    ActivatorID ActivatorID { get; }
    bool IsSatisfied(T state);
}