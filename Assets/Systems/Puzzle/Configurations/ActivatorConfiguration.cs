using UnityEngine;

// Author: Joshua
// Modified by: GitHub Copilot ( April 2026)


// Generic base class for puzzle activator configurations.
// Extend this for different activator types (stones, levers, buttons, etc).
public abstract class ActivatorConfiguration : ScriptableObject
{
    public abstract bool IsSolved(IPuzzleStateProvider state);
}