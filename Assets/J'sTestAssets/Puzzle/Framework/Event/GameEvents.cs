using System;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot (April 2026)
public static class GameEvents
{
    public static event Action<ActivatorState> OnActivatorStateChanged;
    public static event Action<string, float> OnStoneRotationChanged;

    public static void RaiseActivatorStateChanged(string id, object state)
    {
        OnActivatorStateChanged?.Invoke(new ActivatorState(id, state));
    }

    public static void RaiseActivatorStateChanged(ActivatorState state)
    {
        OnActivatorStateChanged?.Invoke(state);
    }

    public static void RaiseStoneRotationChanged(string id, float rotation)
    {
        OnStoneRotationChanged?.Invoke(id, rotation);
        RaiseActivatorStateChanged(id, rotation);
    }
}