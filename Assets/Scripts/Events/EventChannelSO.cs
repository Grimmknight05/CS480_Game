using System;
using UnityEngine;

public abstract class EventChannelSO<T> : ScriptableObject
{
    public event Action<T> OnRaised;
    public T LastValue { get; private set; }
    public bool HasValue { get; private set; }

    public void Raise(T payload)
    {
        LastValue = payload;
        HasValue = true;
        OnRaised?.Invoke(payload);
    }

    void OnDisable()
    {
        // Clear subscribers and replay state on Play Mode exit (§6.5).
        OnRaised = null;
        HasValue = false;
        LastValue = default;
    }
}
