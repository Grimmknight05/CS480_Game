using System;
using UnityEngine;

public abstract class ActivatorStateChannel<T> : ScriptableObject
{
    public event Action<ActivatorID, T> OnStateChanged;

    public void RaiseEvent(ActivatorID id, T state)
    {
        if (id == null)
        {
            Debug.LogWarning($"{name}: RaiseEvent called with null ActivatorID.", this);
            return;
        }

        OnStateChanged?.Invoke(id, state);
    }

    protected virtual void OnDisable()
    {
        OnStateChanged = null;
    }
}