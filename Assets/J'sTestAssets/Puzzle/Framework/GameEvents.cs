using System;

public static class GameEvents
{
    // Generic activator event (works with any activator type)
    public static event Action<ActivatorState> OnActivatorStateChanged;

    // Legacy stone event (kept for backward compatibility)
    public static event Action<string, float> OnStoneRotationChanged;

    public static void RaiseActivatorStateChanged(string id, object state)
    {
        OnActivatorStateChanged?.Invoke(new ActivatorState(id, state));
    }

    public static void RaiseActivatorStateChanged(ActivatorState state)
    {
        OnActivatorStateChanged?.Invoke(state);
    }

    // Legacy method (kept for backward compatibility)
    public static void RaiseStoneRotationChanged(string id, float rotation)
    {
        OnStoneRotationChanged?.Invoke(id, rotation);
        RaiseActivatorStateChanged(id, rotation);
    }
}