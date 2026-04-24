// Author: GitHub Copilot (refactored for generic activator system, April 2026)


// Interface for any requirement that checks if an activator satisfies a condition.
// This allows different activator types (stones, levers, buttons) to have different requirement logic.

public interface IActivatorRequirement
{

    
    string ActivatorID { get; } // Unique ID of the activator this requirement applies to.
    bool IsSatisfied(object activatorState); // Check if the given state satisfies this requirement.
}
