using System;

public static class GameEvents
{
    public static event Action<string, float> OnStoneRotationChanged;

    public static void RaiseStoneRotationChanged(string id, float rotation)
    {
        OnStoneRotationChanged?.Invoke(id, rotation);
    }
}