using UnityEngine;

// Author: Joshua
// Modified by: GitHub Copilot ( April 2026)


// Generic base class for puzzle activator configurations.
// Extend this for different activator types (stones, levers, buttons, etc).

public abstract class ActivatorConfiguration : ScriptableObject
{

    // Get all requirements this configuration needs to be satisfied.
    public abstract IActivatorRequirement[] GetRequirements();
}
