using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Activator Event Channel")]
public class ActivatorEventChannel : ScriptableObject
{
    public event Action<ActivatorID, object> OnEvent;

    public void Raise(ActivatorID id, object state)
    {
        OnEvent?.Invoke(id, state);
    }
}