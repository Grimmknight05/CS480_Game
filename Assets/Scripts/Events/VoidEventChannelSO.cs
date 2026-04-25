using System;
using UnityEngine;

public abstract class VoidEventChannelSO : ScriptableObject
{
    public event Action OnRaised;

    public void Raise() => OnRaised?.Invoke();

    void OnDisable()
    {
        // Clear subscribers on Play Mode exit so they don't leak when Domain Reload is disabled (§6.5).
        OnRaised = null;
    }
}
