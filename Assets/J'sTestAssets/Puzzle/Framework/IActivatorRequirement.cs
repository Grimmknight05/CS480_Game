/// <summary>
/// Interface for any requirement that checks if an activator satisfies a condition.
/// This allows different activator types (stones, levers, buttons) to have different requirement logic.
/// </summary>
public interface IActivatorRequirement
{
    /// <summary>
    /// Unique ID of the activator this requirement applies to.
    /// </summary>
    string ActivatorID { get; }

    /// <summary>
    /// Check if the given state satisfies this requirement.
    /// State can be a float (rotation), bool (button press), int (position), or anything else.
    /// </summary>
    bool IsSatisfied(object activatorState);
}
