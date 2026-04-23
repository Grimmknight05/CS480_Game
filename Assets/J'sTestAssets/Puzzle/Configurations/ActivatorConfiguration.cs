using UnityEngine;

/// <summary>
/// Generic base class for puzzle activator configurations.
/// Extend this for different activator types (stones, levers, buttons, etc).
/// </summary>
public abstract class ActivatorConfiguration : ScriptableObject
{
    /// <summary>
    /// Get all requirements this configuration needs to be satisfied.
    /// </summary>
    public abstract IActivatorRequirement[] GetRequirements();
}
